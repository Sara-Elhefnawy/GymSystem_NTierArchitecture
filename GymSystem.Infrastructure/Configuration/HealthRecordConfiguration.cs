using GymSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

public class HealthRecordConfiguration : IEntityTypeConfiguration<HealthRecord>
{
    public void Configure(EntityTypeBuilder<HealthRecord> builder)
    {
        builder.Property(h => h.BloodType)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .HasMaxLength(20);

        builder.Property(h => h.Height)
            .HasColumnType("decimal(10,2)");

        builder.Property(h => h.Weight)
            .HasColumnType("decimal(10,2)");

        builder.Property(h => h.Note)
            .HasColumnType("varchar")
            .HasMaxLength(500);

        builder.HasOne(h => h.Member)
            .WithOne(m => m.HealthRecord)
            .HasForeignKey<HealthRecord>(h => h.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.MemberId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasQueryFilter(h => !h.IsDeleted);
    }
}
