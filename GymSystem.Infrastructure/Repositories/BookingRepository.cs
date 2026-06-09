using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class BookingRepository(GymAppDbContext dbContext) : Repository<Booking>(dbContext), IBookingRepository
{
    //public async Task<bool> HasUpcomingBookingsAsync(int memberId, DateTime utcNow, CancellationToken ct = default)
    //    => await dbContext.Set<Member>()
    //    .Where(m => m.Id == memberId)
    //    .AnyAsync(m => m.Bookings.Any(b => b.Session.EndDate >= utcNow), ct);

    public async Task<bool> GetWithDetailsAsync(int memberId, DateTime utcNow, CancellationToken ct = default)
        => await dbContext.Set<Booking>()
            .AnyAsync(b => b.MemberId == memberId && b.Session.EndDate >= utcNow, ct);
}
