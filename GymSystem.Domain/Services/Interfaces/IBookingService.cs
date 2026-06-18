using GymSystem.Domain.DTOs.Booking;
using GymSystem.Shared.Common;

namespace GymSystem.Domain.Services.Interfaces;

public interface IBookingService
{
    Task<Result<IReadOnlyList<SessionDTO>>> GetAvailableSessionsAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<SessionInBookingDTO>>> GetMembersForUpcomingSessionAsync(int sessionId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<SessionInBookingDTO>>> GetMembersForOngoingSessionAsync(int sessionId, CancellationToken ct = default);
    Task<Result> CreateAsync(CreateBookingDTO model, CancellationToken ct = default);
    Task<Result> CancelAsync(int memberId, int sessionId, CancellationToken ct = default);
    Task<Result> MarkAttendanceAsync(int memberId, int sessionId, CancellationToken ct = default);
}
