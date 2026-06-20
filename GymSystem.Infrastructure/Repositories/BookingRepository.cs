using GymSystem.Domain.Abstractions.Repositories;
using GymSystem.Infrastructure.Data;
using GymSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Repositories;

public class BookingRepository(GymAppDbContext dbContext) : Repository<Booking>(dbContext), IBookingRepository
{
    public async Task<IEnumerable<Booking>> GetBookingsBySessionIdAsync(int sessionId, CancellationToken ct = default)
        => await dbContext.Bookings
                .Include(b => b.Member)
                .Include(b => b.Session)
                .Where(b => b.SessionId == sessionId)
                .OrderBy(b => b.Member.Name)
                .ToListAsync(ct);
    public async Task<bool> IsMemberAlreadyBookedAsync(int memberId, int sessionId, CancellationToken ct = default)
            => await dbContext.Bookings
                .AnyAsync(b => b.MemberId == memberId && b.SessionId == sessionId, ct);

    public async Task<bool> GetWithMemberDetailsAsync(int memberId, DateTime now, CancellationToken ct = default)
        => await dbContext.Set<Booking>()
            .AnyAsync(b => b.MemberId == memberId && b.Session.EndDate >= now, ct);

    public async Task<bool> CancelBookingAsync(int memberId, int sessionId, CancellationToken ct = default)
    {
        var booking = await dbContext.Bookings
                .FirstOrDefaultAsync(b => b.MemberId == memberId && b.SessionId == sessionId, ct);

        if (booking == null)
            return false;

        booking.IsDeleted = true;
        booking.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkAttendanceAsync(int memberId, int sessionId, CancellationToken ct = default)
    {
        var booking = await dbContext.Bookings
                .FirstOrDefaultAsync(b => b.MemberId == memberId && b.SessionId == sessionId, ct);

        if (booking == null)
            return false;

        booking.IsAttended = true;
        await dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<Booking>> GetUpcomingBookingsByMemberIdAsync(int memberId, CancellationToken ct = default)
        => await dbContext.Bookings
            .Include(b => b.Session)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId
                && !b.IsDeleted
                && !b.IsAttended
                && b.Session.StartDate >= DateTime.Now)
            .OrderBy(b => b.Session.StartDate)
            .ToListAsync(ct);

    public async Task<IEnumerable<Booking>> GetBookingsWithActiveMembershipForSessionAsync(int sessionId, CancellationToken ct = default)
        => await dbContext.Bookings
            .Include(b => b.Member)
                .ThenInclude(m => m.Memberships.Where(mem => !mem.IsDeleted && mem.EndDate >= DateOnly.FromDateTime(DateTime.Now)))
            .Include(b => b.Session)
            .Where(b => b.SessionId == sessionId
                && !b.IsDeleted
                && b.Member != null
                && b.Member.Memberships.Any(mem => !mem.IsDeleted && mem.EndDate >= DateOnly.FromDateTime(DateTime.Now)))
            .OrderBy(b => b.Member.Name)
            .ToListAsync(ct);
}
