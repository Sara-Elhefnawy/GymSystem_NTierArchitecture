using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

public class MemberConfiguration : GymUserConfigurations<Member>
{
    public override void Configure(EntityTypeBuilder<Member> builder)
    {
        base.Configure(builder);

        builder.Property(m => m.JoinDate)
            .HasDefaultValueSql("GETDATE()");

        builder.OwnsOne(m => m.HealthRecord, healthRecord =>
        {
            healthRecord.Property(h => h.BloodType)
                .HasColumnType("varchar")
                .HasMaxLength(5);

            healthRecord.Property(h => h.Height)
                .HasColumnType("decimal(10,2)");

            healthRecord.Property(h => h.Weight)
                .HasColumnType("decimal(10,2)");

            healthRecord.Property(h => h.Note)
                .HasColumnType("varchar")
                .HasMaxLength(500);
        });
    }
}
