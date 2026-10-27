using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");

            builder.HasKey(x => x.AppointmentId);
            builder.Property(a => a.AppointmentId).ValueGeneratedOnAdd();

            builder.Property(a => a.AppointmentDateTime)
                .HasColumnType("datetime2(2)")
                .IsRequired();

            builder.Property(e=>e.Status).IsRequired();

            builder.HasOne(a => a.Patient)
         .WithMany(p => p.Appointments)
         .HasForeignKey(a => a.PatientId)
         .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.MedicalRecord)
                .WithOne(r => r.Appointment)
                .HasForeignKey<Appointment>(a => a.MedicalRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Payment)
                .WithOne(p => p.Appointment)
                .HasForeignKey<Appointment>(a => a.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.DoctorId, a.AppointmentDateTime })
    .IsUnique()
    .HasFilter($"[Status] <> {(byte)AppointmentStatus.Cancelled}");

            // AppointmentConfiguration.cs, add inside Configure
            builder.HasIndex(a => new { a.PatientId, a.AppointmentDateTime });

        }
    }
}
