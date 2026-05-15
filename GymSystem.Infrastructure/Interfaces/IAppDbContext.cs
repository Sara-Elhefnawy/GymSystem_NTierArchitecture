using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Domain.Interfaces;

public interface IAppDbContext
{
    DbSet<Member> Members { get; set; }
    DbSet<CheckIn> CheckIns { get; set; }
    DbSet<Booking> Bookings { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
