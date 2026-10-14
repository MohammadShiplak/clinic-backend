using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.Property(d => d.Specialization).HasMaxLength(100);

            builder.Property(d => d.Biography).HasMaxLength(2000);
            builder.Property(d => d.ClinicRoom).HasMaxLength(50);


            builder.Property(d => d.ProfileImageUrl).HasMaxLength(100);


            builder.HasIndex(d=>d.Specialization);  


            builder.HasOne(d => d.Person)
                .WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
