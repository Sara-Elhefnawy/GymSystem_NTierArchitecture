using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Session_Capacity",
                "[Capacity] BETWEEN 1 AND 25");

            t.HasCheckConstraint("CK_Session_Dates",
                "[EndDate] > [StartDate]");
        });

        // Cannot delete sessions that are still in the future

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
