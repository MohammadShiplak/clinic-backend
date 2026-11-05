using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations;

public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("UserAccounts");

        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Role).IsRequired();   // stored as tinyint, like AppointmentStatus
        builder.Property(u => u.CreatedAtUtc).HasColumnType("datetime2(2)");
        builder.Property(u => u.LastLoginAtUtc).HasColumnType("datetime2(2)");

        // The real guarantee that two accounts can't share an email.
        builder.HasIndex(u => u.Email).IsUnique();

        // One-to-one: EF automatically adds a unique index on PersonId.
        builder.HasOne(u => u.Person)
            .WithOne(p => p.UserAccount)
            .HasForeignKey<UserAccount>(u => u.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}