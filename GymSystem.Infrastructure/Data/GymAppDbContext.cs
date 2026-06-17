using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Identities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Infrastructure.Data;

public class GymAppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public GymAppDbContext(DbContextOptions<GymAppDbContext> options)
        : base(options) { }

    public DbSet<GymUser> GymUsers { get; set; } = default!;

    // Members and Trainers become derived sets (using => Set<T>())
    // they query the same table but filtered by discriminator
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Membership> Memberships { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Plan> Plans { get; set; } = default!;
    public DbSet<Session> Sessions { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymAppDbContext).Assembly);
    
        base.OnModelCreating(modelBuilder);
    }
}
