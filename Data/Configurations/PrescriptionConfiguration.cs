using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.ToTable("Prescriptions", t =>
      t.HasCheckConstraint("CK_Prescriptions_Dates", "[EndDate] >= [StartDate]"));

            builder.HasOne(p => p.MedicalRecord)
                .WithMany(r => r.Prescriptions)
                .HasForeignKey(p => p.MedicalRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.MedicationName).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Dosage).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Frequency).IsRequired().HasMaxLength(50);
            builder.Property(p => p.SpecialInstructions).HasMaxLength(200);

        }
    }
}
