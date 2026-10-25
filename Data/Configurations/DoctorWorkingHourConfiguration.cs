using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class DoctorWorkingHourConfiguration : IEntityTypeConfiguration<DoctorWorkingHour>
    {
      

        public void Configure(EntityTypeBuilder<DoctorWorkingHour> builder)
        {
            builder.ToTable("DoctorWorkingHours", t =>
          t.HasCheckConstraint("CK_DoctorWorkingHours_Times", "[EndTime] > [StartTime]"));

            builder.Property(x => x.DayOfWeek).HasConversion<byte>();

            builder.HasOne(x => x.Doctor)
                .WithMany(d => d.WorkingHours)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.DoctorId, x.DayOfWeek });
        }
    }
}
