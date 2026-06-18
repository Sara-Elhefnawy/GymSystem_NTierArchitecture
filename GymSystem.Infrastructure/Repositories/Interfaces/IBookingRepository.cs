using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetBookingsBySessionIdAsync(int sessionId, CancellationToken ct = default);
    Task<IEnumerable<Booking>> GetBookingsByMemberIdAsync(int memberId, CancellationToken ct = default);
    //Task<bool> HasUpcomingBookingsAsync(int memberId, DateTime utcNow, CancellationToken ct = default);
    Task<bool> IsMemberAlreadyBookedAsync(int memberId, int sessionId, CancellationToken ct = default);
    Task<bool> GetWithMemberDetailsAsync(int id, DateTime utcNow, CancellationToken ct = default);
    Task<bool> CancelBookingAsync(int memberId, int sessionId, CancellationToken ct = default);
    Task<bool> MarkAttendanceAsync(int memberId, int sessionId, CancellationToken ct = default);
    Task<int> GetBookingCountBySessionIdAsync(int sessionId, CancellationToken ct = default);
}
