using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetBookingsBySessionIdAsync(int sessionId, CancellationToken ct = default);
    Task<bool> IsMemberAlreadyBookedAsync(int memberId, int sessionId, CancellationToken ct = default);
    Task<bool> GetWithMemberDetailsAsync(int id, DateTime now, CancellationToken ct = default);
    Task<bool> CancelBookingAsync(int memberId, int sessionId, CancellationToken ct = default);
    Task<bool> MarkAttendanceAsync(int memberId, int sessionId, CancellationToken ct = default);

    Task<IEnumerable<Booking>> GetUpcomingBookingsByMemberIdAsync(int memberId, CancellationToken ct = default);
}
