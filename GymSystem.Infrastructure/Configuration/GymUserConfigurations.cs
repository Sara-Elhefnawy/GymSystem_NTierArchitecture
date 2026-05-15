using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

public class GymUserConfigurations<T> : IEntityTypeConfiguration<T> where T : GymUser
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.Name)
               .HasColumnType("varchar")
               .HasMaxLength(50);

        builder.Property(x => x.Email)
               .HasColumnType("varchar")
               .HasMaxLength(100);

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Street)
                   .HasColumnName("Street")
                   .HasColumnType("varchar")
                   .HasMaxLength(30);

            address.Property(a => a.City)
                   .HasColumnType("varchar")
                   .HasColumnName("City")
                   .HasMaxLength(30);

            address.Property(a => a.BuildingNumber)
                   .HasColumnName("BuildingNumber");
        });

        builder.Property(x => x.Phone)
               .HasColumnType("varchar")
               .HasMaxLength(11);

        builder.Property(x => x.DateOfBirth)
               .HasColumnType("date");

        builder.Property(x => x.Gender)
               .HasConversion<int>();

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Phone).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("GymUser_EmailCheck", "Email LIKE '_%@_%._%'");
            t.HasCheckConstraint("GymUser_PhoneCheck",
                "[Phone] LIKE '010%' OR [Phone] LIKE '011%' OR [Phone] LIKE '012%' OR [Phone] LIKE '015%'");
        });
    }
}
