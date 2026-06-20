using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Booking;
using GymSystem.Domain.DTOs.CheckIn;

namespace GymSystem.Domain.Abstractions.Services;

public interface IBookingService
{
    Task<Result<IReadOnlyList<IndexBookingDTO>>> GetAvailableSessionsAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<SessionInBookingDTO>>> GetMembersForSessionAsync(int sessionId, CancellationToken ct = default);
    Task<Result> CreateAsync(CreateBookingDTO model, CancellationToken ct = default);
    Task<Result> CancelAsync(int memberId, int sessionId, CancellationToken ct = default);
    Task<Result> MarkAttendanceAsync(int memberId, int sessionId, CancellationToken ct = default);

    Task<Result<ResultCheckInDTO>> CheckInViaQRAsync(int memberId, CancellationToken ct = default);
}
