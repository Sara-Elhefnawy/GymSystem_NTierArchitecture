using GymSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasBaseType<GymUser>();

        builder.Property(m => m.JoinDate)
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(m => m.HealthRecord)
            .WithOne(h => h.Member)
            .HasForeignKey<HealthRecord>(h => h.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
