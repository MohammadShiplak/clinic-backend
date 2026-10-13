using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
   

        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.Property(r => r.VisitDescription).HasMaxLength(2000);
            builder.Property(r => r.Diagnosis).HasMaxLength(500);
            builder.Property(r => r.AdditionalNotes).HasMaxLength(2000);
        }
    }
}
