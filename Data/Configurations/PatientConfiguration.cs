
using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.Property(p => p.IsActive).HasDefaultValue(true);


            builder.HasIndex(p => p.IsActive);   // see the note below


            builder.HasOne(p => p.Person)
                .WithOne(p => p.Patient)
                .HasForeignKey<Patient>(p => p.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
