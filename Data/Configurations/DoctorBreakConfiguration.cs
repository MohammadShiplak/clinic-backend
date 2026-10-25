using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagementSystem.API.Data.Configurations
{
    public class DoctorBreakConfiguration : IEntityTypeConfiguration<DoctorBreak>
    {
        public void Configure(EntityTypeBuilder<DoctorBreak> builder)
        {
            builder.ToTable("DoctorBreakHours", t =>
        t.HasCheckConstraint("CK_DoctorBreakHours_Times", "[EndTime] > [StartTime]"));

            builder.Property(x => x.DayOfWeek).HasConversion<byte>();

            builder.HasOne(x => x.Doctor)
           .WithMany(d => d.Breaks)
           .HasForeignKey(x => x.DoctorId)
           .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.DoctorId, x.DayOfWeek });
        }
    }
}
