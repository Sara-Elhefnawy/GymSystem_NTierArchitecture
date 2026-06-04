using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.HasBaseType<GymUser>();

        builder.Property(t => t.HireDate)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(t => t.Specialty)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
