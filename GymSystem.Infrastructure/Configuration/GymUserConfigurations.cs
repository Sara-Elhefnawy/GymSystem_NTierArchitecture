using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.Infrastructure.Configuration;

// Why remove the generic?
/*
 * Generic version meant each derived type (Member, Trainer) 
 *      got its OWN copy of the configuration
 * Non-generic version applies configuration ONCE to the base GymUser entity
 * EF Core's TPH (Table Per Hierarchy) mapping means all GymUser properties are in ONE table - 
 *      so configure them once, not multiple times
 */

public class GymUserConfigurations : IEntityTypeConfiguration<GymUser>
{
    public virtual void Configure(EntityTypeBuilder<GymUser> builder)
    {
        // don't need to use this code in OnModelConfiguring and following separation of concerns
        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.HasDiscriminator<string>("UserType")
            .HasValue<Member>("Member")
            .HasValue<Trainer>("Trainer");




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
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.Phone)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("GymUser_EmailCheck", "Email LIKE '_%@_%._%'");
            t.HasCheckConstraint("GymUser_PhoneCheck",
                "[Phone] LIKE '010%' OR [Phone] LIKE '011%' OR [Phone] LIKE '012%' OR [Phone] LIKE '015%'");
        });
    }
}
