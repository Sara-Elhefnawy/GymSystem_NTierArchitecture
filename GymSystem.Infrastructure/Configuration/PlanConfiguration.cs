using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(200);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(10,2)");

        builder.ToTable(Tb =>
        {
            Tb.HasCheckConstraint("PlanDurationCheck",
                "DurationDays Between 1 and 365");
        });

        builder.HasQueryFilter(h => !h.IsDeleted);
    }
}
