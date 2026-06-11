using GymSystem.Infrastructure.Entities;

namespace GymSystem.Infrastructure.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<bool> HasUpcomingBookingsAsync(int memberId, DateTime utcNow, CancellationToken ct = default);
    Task<bool> GetWithDetailsAsync(int id, DateTime utcNow, CancellationToken ct = default);
}
