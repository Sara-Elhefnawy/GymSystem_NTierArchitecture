using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.HasOne(m => m.Member)
            .WithMany(me => me.Memberships)
            .HasForeignKey(m => m.MemberId);

        builder.HasOne(m => m.Plan)
            .WithMany(p => p.Memberships)
            .HasForeignKey(m => m.PlanId);

        builder.Property(m => m.StartDate)
            .HasDefaultValueSql("GETDATE()");

        builder.HasIndex(m => new { m.MemberId, m.PlanId }).IsUnique();

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
