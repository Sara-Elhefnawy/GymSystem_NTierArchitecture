using GymSystem.Domain.Entities;

namespace GymSystem.Domain.Abstractions.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetBookingsBySessionIdAsync(int sessionId, CancellationToken ct = default);
    Task<bool> IsMemberAlreadyBookedAsync(int memberId, int sessionId, CancellationToken ct = default);
    Task<bool> GetWithMemberDetailsAsync(int id, DateTime now, CancellationToken ct = default);
    Task<bool> CancelBookingAsync(int memberId, int sessionId, CancellationToken ct = default);
    Task<bool> MarkAttendanceAsync(int memberId, int sessionId, CancellationToken ct = default);

    Task<IEnumerable<Booking>> GetUpcomingBookingsByMemberIdAsync(int memberId, CancellationToken ct = default);

    Task<IEnumerable<Booking>> GetBookingsWithActiveMembershipForSessionAsync(int sessionId, CancellationToken ct = default);
}
