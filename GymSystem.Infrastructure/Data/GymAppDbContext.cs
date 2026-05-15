using GymSystem.Domain.Interfaces;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Data;

public class GymAppDbContext : DbContext, IAppDbContext
{
    public GymAppDbContext(DbContextOptions<GymAppDbContext> options)
        : base(options) { }

    public DbSet<Member> Members { get; set; } = default!;
    public DbSet<Trainer> Trainers { get; set; } = default!;
    public DbSet<Membership> Memberships { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Plan> Plans { get; set; } = default!;
    public DbSet<Session> Sessions { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;


    public DbSet<CheckIn> CheckIns { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GymUser>().HasQueryFilter(u => !u.IsDeleted);

        modelBuilder.Entity<GymUser>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Member>("Member")
            .HasValue<Trainer>("Trainer");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymAppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
