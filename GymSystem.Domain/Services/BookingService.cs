using GymSystem.Domain.DTOs.Booking;
using GymSystem.Domain.DTOs.CheckIn;
using GymSystem.Domain.QRCode;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.UnitOfWorks;
using GymSystem.Shared.Common;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class BookingService(IUnitOfWork uow, ILogger<BookingService> logger, IQrService qrService) : IBookingService
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger<BookingService> _logger = logger;
    private readonly IQrService _qrService = qrService;

    public async Task<Result<IReadOnlyList<IndexBookingDTO>>> GetAvailableSessionsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting available sessions");

            var sessions = await _uow.Sessions.GetAllWithBookingsAsync(ct);

            var viewModels = sessions.Select(s => new IndexBookingDTO
            {
                Id = s.Id,
                CategoryName = s.Category?.Name ?? "Uncategorized",
                Description = s.Description ?? string.Empty,
                TrainerName = s.Trainer?.Name ?? "Unassigned",
                DateDisplay = s.StartDate.ToString("dddd, MMM dd, yyyy"),
                TimeRangeDisplay = $"{s.StartDate:hh:mm tt} - {s.EndDate:hh:mm tt}",
                Duration = $"{s.EndDate.Subtract(s.StartDate).TotalMinutes} min",
                Capacity = s.Capacity,
                Status = GetSessionStatus(s),
                AvailableSlots = s.Capacity - (s.Bookings?.Count ?? 0)
            }).ToList();

            _logger.LogInformation("Retrieved {Count} sessions", viewModels.Count());

            return Result.Ok<IReadOnlyList<IndexBookingDTO>>(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available sessions");
            return Result.Fail<IReadOnlyList<IndexBookingDTO>>("Failed to retrieve sessions", "DATABASE_ERROR");
        }
    }

    public async Task<Result<IReadOnlyList<SessionInBookingDTO>>> GetMembersForUpcomingSessionAsync(int sessionId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting members for upcoming session {SessionId}", sessionId);

            var bookings = await _uow.Bookings.GetBookingsBySessionIdAsync(sessionId, ct);

            var viewModels = bookings.Select(b => new SessionInBookingDTO
            {
                MemberId = b.MemberId,
                MemberName = b.Member?.Name ?? "Unknown",
                SessionId = b.SessionId,
                IsAttended = b.IsAttended,
                AttendanceMarkedAt = b.AttendanceMarkedAt, // Map this
                BookingDate = b.BookingDate
            }).ToList();

            _logger.LogInformation("Retrieved {Count} members for session {SessionId}", viewModels.Count(), sessionId);

            return Result.Ok<IReadOnlyList<SessionInBookingDTO>>(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members for session {SessionId}", sessionId);
            return Result.Fail<IReadOnlyList<SessionInBookingDTO>>("Failed to retrieve members", "DATABASE_ERROR");
        }
    }

    public async Task<Result<IReadOnlyList<SessionInBookingDTO>>> GetMembersForOngoingSessionAsync(int sessionId, CancellationToken ct = default)
    {
        return await GetMembersForUpcomingSessionAsync(sessionId, ct);
    }

    public async Task<Result> CreateAsync(CreateBookingDTO model, CancellationToken ct = default)
    {
        _logger.LogInformation("=== BookingService.CreateBookingAsync called ===");
        _logger.LogInformation("Model: MemberId={MemberId}, SessionId={SessionId}", model.MemberId, model.SessionId);

        try
        {
            var session = await _uow.Sessions.GetByIdWithBookingsAsync(model.SessionId, ct);
            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found", model.SessionId);
                return Result.Fail("Session not found", "SESSION_NOT_FOUND");
            }
            _logger.LogInformation("Session found: Id={Id}, Capacity={Capacity}", session.Id, session.Capacity);

            _logger.LogInformation("Validating member exists...");
            var member = await _uow.Members.GetByIdAsync(model.MemberId, ct);
            if (member == null)
            {
                _logger.LogWarning("Member {MemberId} not found", model.MemberId);
                return Result.Fail("Member not found", "MEMBER_NOT_FOUND");
            }
            _logger.LogInformation("Member found: Id={Id}, Name={Name}", member.Id, member.Name);

            _logger.LogInformation("Checking if member has active membership...");
            var hasActivePlan = await _uow.Memberships.IsMemberAlreadyHasActivePlanAsync(model.MemberId, ct);
            _logger.LogInformation("Has active plan: {HasActivePlan}", hasActivePlan);

            if (!hasActivePlan)
            {
                _logger.LogWarning("Member {MemberId} does not have an active membership", model.MemberId);
                return Result.Fail("Member does not have an active membership", "NO_ACTIVE_MEMBERSHIP");
            }

            _logger.LogInformation("Checking if member is already booked...");
            var alreadyBooked = await _uow.Bookings.IsMemberAlreadyBookedAsync(model.MemberId, model.SessionId, ct);
            _logger.LogInformation("Already booked: {AlreadyBooked}", alreadyBooked);

            if (alreadyBooked)
            {
                _logger.LogWarning("Member {MemberId} already booked for session {SessionId}", model.MemberId, model.SessionId);
                return Result.Fail("Member already booked for this session", "ALREADY_BOOKED");
            }

            var bookingCount = session.Bookings?.Count(b => !b.IsDeleted) ?? 0;
            var availableSlots = session.Capacity - bookingCount;

            _logger.LogInformation("Session {SessionId} - Capacity: {Capacity}, Bookings: {Bookings}, Available: {Available}",
                session.Id, session.Capacity, bookingCount, availableSlots);

            if (availableSlots <= 0)
            {
                _logger.LogWarning("Session {SessionId} is full", model.SessionId);
                return Result.Fail("Session is at full capacity", "SESSION_FULL");
            }


            var booking = new Booking
            {
                MemberId = model.MemberId,
                SessionId = model.SessionId,
                BookingDate = DateTime.Now,
                IsAttended = false
            };

            await _uow.Bookings.AddAsync(booking, ct);
            _logger.LogInformation("Booking added to repository");

            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation("Changes saved to database");

            _logger.LogInformation("Booking created successfully for member {MemberId} in session {SessionId}",
                model.MemberId, model.SessionId);

            // ✅ FIX: Generate QR code after successful creation
            try
            {
                // Generate QR code for this booking
                var qrResult = await _qrService.GenerateMemberQrPngAsync(model.MemberId, ct);
                if (qrResult.IsSuccess)
                {
                    _logger.LogInformation("QR code generated for member {MemberId}, session {SessionId}",
                        model.MemberId, model.SessionId);
                }
                else
                {
                    _logger.LogWarning("Failed to generate QR code for member {MemberId}, session {SessionId}: {Error}",
                        model.MemberId, model.SessionId, qrResult.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate QR code for member {MemberId}, session {SessionId}",
                    model.MemberId, model.SessionId);
            }

            return Result.Ok("Booking created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateBookingAsync for MemberId: {MemberId}, SessionId: {SessionId}",
                model.MemberId, model.SessionId);
            return Result.Fail($"Failed to create booking: {ex.Message}", "DATABASE_ERROR");
        }
    }

    public async Task<Result> CancelAsync(int memberId, int sessionId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Cancelling booking for member {MemberId} in session {SessionId}",
                memberId, sessionId);

            var result = await _uow.Bookings.CancelBookingAsync(memberId, sessionId, ct);

            if (!result)
            {
                _logger.LogWarning("Booking not found for member {MemberId} in session {SessionId}",
                    memberId, sessionId);
                return Result.Fail("Booking not found", "BOOKING_NOT_FOUND");
            }

            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Booking cancelled successfully for member {MemberId} in session {SessionId}",
                memberId, sessionId);

            return Result.Ok("Booking cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking for member {MemberId} in session {SessionId}",
                memberId, sessionId);
            return Result.Fail("Failed to cancel booking", "DATABASE_ERROR");
        }
    }

    public async Task<Result> MarkAttendanceAsync(int memberId, int sessionId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Marking attendance for member {MemberId} in session {SessionId}",
                memberId, sessionId);

            var result = await _uow.Bookings.MarkAttendanceAsync(memberId, sessionId, ct);

            if (!result)
            {
                _logger.LogWarning("Booking not found for member {MemberId} in session {SessionId}",
                    memberId, sessionId);
                return Result.Fail("Booking not found", "BOOKING_NOT_FOUND");
            }

            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Attendance marked successfully for member {MemberId} in session {SessionId}",
                memberId, sessionId);

            return Result.Ok("Attendance marked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking attendance for member {MemberId} in session {SessionId}",
                memberId, sessionId);
            return Result.Fail("Failed to mark attendance", "DATABASE_ERROR");
        }
    }

    public async Task<Result<ResultCheckInDTO>> CheckInViaQRAsync(int memberId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing QR check-in for member {MemberId}", memberId);

            // 1. Validate member exists
            var member = await _uow.Members.GetByIdAsync(memberId, ct);
            if (member == null)
            {
                return Result.Fail<ResultCheckInDTO>("Member not found", "MEMBER_NOT_FOUND");
            }

            // 2. Check active membership
            var hasActivePlan = await _uow.Memberships.IsMemberAlreadyHasActivePlanAsync(memberId, ct);
            if (!hasActivePlan)
            {
                return Result.Fail<ResultCheckInDTO>("Member does not have an active membership", "NO_ACTIVE_MEMBERSHIP");
            }

            // 3. Find the CURRENT active session
            var now = DateTime.Now;
            var currentSession = await _uow.Sessions.GetActiveSessionAtTimeAsync(now, ct);

            if (currentSession == null)
            {
                return Result.Fail<ResultCheckInDTO>("No active session right now", "NO_ACTIVE_SESSION");
            }

            // 4. Check capacity
            var bookingCount = currentSession.Bookings?.Count(b => !b.IsDeleted) ?? 0;
            var availableSlots = currentSession.Capacity - bookingCount;

            _logger.LogInformation("Session {SessionId} - Capacity: {Capacity}, Bookings: {Bookings}, Available: {Available}",
                currentSession.Id, currentSession.Capacity, bookingCount, availableSlots);

            if (availableSlots <= 0)
            {
                return Result.Fail<ResultCheckInDTO>("Session is full. No available slots.", "SESSION_FULL");
            }

            // 5. Check if member already has a booking for this session
            var existingBooking = currentSession.Bookings?.FirstOrDefault(b => b.MemberId == memberId && !b.IsDeleted);

            if (existingBooking != null)
            {
                if (existingBooking.IsAttended)
                {
                    return Result.Ok(new ResultCheckInDTO
                    {
                        MemberName = member.Name,
                        SessionName = currentSession.Category?.Name ?? "Session",
                        IsAlreadyAttended = true,
                        WasAutoBooked = false
                    });
                }

                existingBooking.IsAttended = true;
                existingBooking.AttendanceMarkedAt = DateTime.Now;
                await _uow.SaveChangesAsync(ct);

                return Result.Ok(new ResultCheckInDTO
                {
                    MemberName = member.Name,
                    SessionName = currentSession.Category?.Name ?? "Session",
                    IsAlreadyAttended = false,
                    WasAutoBooked = false
                });
            }

            // 6. No booking exists - AUTO-CREATE (Walk-in)
            var newBooking = new Booking
            {
                MemberId = memberId,
                SessionId = currentSession.Id,
                BookingDate = DateTime.Now,
                IsAttended = true,
                AttendanceMarkedAt = DateTime.Now
            };

            await _uow.Bookings.AddAsync(newBooking, ct);
            await _uow.SaveChangesAsync(ct);

            // Generate QR code for future use
            try
            {
                await _qrService.GenerateMemberQrPngAsync(memberId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate QR for walk-in booking");
            }

            return Result.Ok(new ResultCheckInDTO
            {
                MemberName = member.Name,
                SessionName = currentSession.Category?.Name ?? "Session",
                IsAlreadyAttended = false,
                WasAutoBooked = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing QR check-in for member {MemberId}", memberId);
            return Result.Fail<ResultCheckInDTO>("An error occurred during check-in", "CHECKIN_ERROR");
        }
    }

    private string GetSessionStatus(Session session)
    {
        var now = DateTime.Now;
        if (now < session.StartDate)
            return "Upcoming";
        else if (now >= session.StartDate && now <= session.EndDate)
            return "Ongoing";
        else
            return "Completed";
    }
}
