using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class DoctorTimeOffConfiguration : IEntityTypeConfiguration<DoctorTimeOff>
    {
        public void Configure(EntityTypeBuilder<DoctorTimeOff> builder)
        {
            builder.ToTable("DoctorTimeOffs", t =>
            t.HasCheckConstraint("CK_DoctorTimeOffs_Dates", "[EndDateTime] > [StartDateTime]"));

            builder.Property(x => x.Reason).HasMaxLength(200);

            builder.HasOne(x => x.Doctor)
                .WithMany(d => d.TimeOff)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.DoctorId, x.StartDateTime });
        }
    }
}
