using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

public class ChechInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.Property(c => c.CheckInTime)
                  .HasDefaultValueSql("GETDATE()");

        //builder.HasOne(c => c.Member)
        //      .WithMany(m => m.CheckIns)
        //      .HasForeignKey(c => c.MemberId)
        //      .OnDelete(DeleteBehavior.Cascade);
    }
}
