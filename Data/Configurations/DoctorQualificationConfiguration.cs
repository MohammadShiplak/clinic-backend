using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class DoctorQualificationConfiguration : IEntityTypeConfiguration<DoctorQualification>
    {
        public void Configure(EntityTypeBuilder<DoctorQualification> builder)
        {
          builder.Property(q=>q.Title).IsRequired().HasMaxLength(150);

          builder.Property(q => q.Institution).HasMaxLength(150);

            builder.HasOne(q=>q.Doctor)
                .WithMany(d=>d.Qualifications)
                .HasForeignKey(q=>q.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
