using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {

            builder.ToTable("Notifications");

            builder.Property(n => n.Title).IsRequired().HasMaxLength(100);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(500);
            builder.Property(n => n.Type).IsRequired();
            builder.Property(n => n.CreatedAtUtc).HasColumnType("datetime2(2)");
            builder.Property(n => n.CreatedAtUtc)
    .HasColumnType("datetime2(2)")
    .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.HasOne(n => n.Person)
                .WithMany()
                .HasForeignKey(n => n.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.Appointment)
                .WithMany()
                .HasForeignKey(n => n.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Serves: "my notifications, newest first" and "my unread count"
            builder.HasIndex(n => new { n.PersonId, n.IsRead, n.CreatedAtUtc });
            // Hard guarantee: at most one reminder per appointment per person (even with a bug or 2 servers)
            builder.HasIndex(n => new { n.AppointmentId, n.PersonId, n.Type })
                .IsUnique()
                .HasFilter($"[Type] = {(byte)NotificationType.AppointmentReminder}");
        }
    }
}
