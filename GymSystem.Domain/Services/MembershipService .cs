using GymSystem.Domain.DTOs.Membership;
using GymSystem.Domain.QRCode;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Attachments;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.UnitOfWorks;
using GymSystem.Shared.Common;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class MembershipService(IUnitOfWork uow, IQrService qrService, IAttachmentService attachmentService, ILogger<MembershipService> logger) : IMembershipService
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger<MembershipService> _logger = logger;
    private readonly IQrService _qrService = qrService;

    public async Task<Result<IEnumerable<IndexMembershipDTO>>> GetActiveMembershipsAsync(CancellationToken ct = default)
    {
        try
        {
            var memberships = await _uow.Memberships.GetActiveMembershipsAsync(ct);

            var dtos = memberships.Select(m => new IndexMembershipDTO
            {
                Id = m.Id,
                MemberId = m.MemberId,
                MemberName = m.Member?.Name ?? "Unknown",
                PlanName = m.Plan?.Name ?? "Unknown",
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Photo = m.Member?.Photo
            });

            return Result.Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active memberships");
            return Result.Fail<IEnumerable<IndexMembershipDTO>>("DATABASE_ERROR", "Failed to get memberships");
        }
    }

    public async Task<Result> CreateMembershipAsync(CreateMembershipDTO model, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating membership for member {MemberId} with plan {PlanId}",
                model.MemberId, model.PlanId);

            // Validate member exists
            var member = await _uow.Members.GetByIdAsync(model.MemberId, ct);
            if (member == null)
            {
                _logger.LogWarning("Member {MemberId} not found", model.MemberId);
                return Result.Fail("Member not found", "MEMBER_NOT_FOUND");
            }

            // Validate plan exists
            var plan = await _uow.Plans.GetByIdAsync(model.PlanId, ct);
            if (plan == null)
            {
                _logger.LogWarning("Plan {PlanId} not found", model.PlanId);
                return Result.Fail("Plan not found", "PLAN_NOT_FOUND");
            }

            // Check if member already has active membership
            if (await _uow.Memberships.IsMemberAlreadyHasActivePlanAsync(model.MemberId, ct))
            {
                _logger.LogWarning("Member {MemberId} already has an active membership", model.MemberId);
                return Result.Fail("Member already has an active membership", "ALREADY_ACTIVE");
            }

            // Create membership
            var membership = new Membership
            {
                MemberId = model.MemberId,
                PlanId = model.PlanId,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(plan.DurationDays)),
            };

            await _uow.Memberships.AddAsync(membership, ct);
            await _uow.SaveChangesAsync(ct);

            await GenerateQrCodesForMemberAsync(model.MemberId, ct);

            _logger.LogInformation("Membership created successfully for member {MemberId}", model.MemberId);

            return Result.Ok("Membership created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating membership for member {MemberId}", model.MemberId);
            return Result.Fail("Failed to create membership", "DATABASE_ERROR");
        }
    }

    /// Generates QR codes for all upcoming sessions the member is booked for
    private async Task GenerateQrCodesForMemberAsync(int memberId, CancellationToken ct)
    {
        try
        {
            // Get all upcoming bookings for this member
            var bookings = await _uow.Bookings.GetUpcomingBookingsByMemberIdAsync(memberId, ct);

            if (bookings == null || !bookings.Any())
            {
                _logger.LogInformation("No upcoming bookings found for member {MemberId}", memberId);
                return;
            }

            _logger.LogInformation("Generating QR codes for {Count} bookings for member {MemberId}",
                bookings.Count(), memberId);

            foreach (var booking in bookings)
            {
                try
                {
                    // Generate QR code for this specific session
                    var qrResult = await _qrService.GenerateMemberQrPngAsync(memberId, ct);

                    if (qrResult.IsSuccess)
                    {
                        _logger.LogInformation("QR code generated for member {MemberId}, session {SessionId}",
                            memberId, booking.SessionId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to generate QR for member {MemberId}, session {SessionId}: {Error}",
                            memberId, booking.SessionId, qrResult.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating QR for member {MemberId}, session {SessionId}",
                        memberId, booking.SessionId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR codes for member {MemberId}", memberId);
        }
    }

    public async Task<Result> CancelMembershipAsync(int membershipId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation($"Attempting to cancel membership with ID: {membershipId}");

            // Get the membership with includes
            var membership = await _uow.Memberships.GetByIdWithIncludesAsync(membershipId, ct);

            if (membership == null)
            {
                _logger.LogWarning($"Membership with ID {membershipId} not found");
                return Result.Fail("MEMBERSHIP_NOT_FOUND", "Membership not found");
            }

            if (membership.IsDeleted)
            {
                _logger.LogWarning($"Membership with ID {membershipId} is already deleted");
                return Result.Fail("ALREADY_CANCELLED", "Membership is already cancelled");
            }

            // Cancel the membership
            var result = await _uow.Memberships.CancelMembershipByIdAsync(membershipId, ct);

            if (!result)
            {
                return Result.Fail("CANCEL_FAILED", "Failed to cancel membership");
            }

            _logger.LogInformation($"Membership {membershipId} cancelled successfully");

            // Delete the QR code for this member
            try
            {
                // Use the new delete method that works with predictable file naming
                var qrDeleteResult = await _qrService.DeleteMemberQrCodeAsync(membership.MemberId, ct);

                if (qrDeleteResult.IsSuccess)
                {
                    _logger.LogInformation($"QR code deleted successfully for member {membership.MemberId}");
                }
                else
                {
                    _logger.LogWarning($"Failed to delete QR code for member {membership.MemberId}: {qrDeleteResult.Error}");
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the operation if QR deletion fails
                _logger.LogWarning(ex, $"Exception deleting QR code for member {membership.MemberId}");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cancelling membership {membershipId}");
            return Result.Fail("DATABASE_ERROR", $"Failed to cancel membership: {ex.Message}");
        }
    }
}
