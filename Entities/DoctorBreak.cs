namespace ClinicManagementSystem.API.Entities
{
    public class DoctorBreak
    {
        public int DoctorBreakId { get; set; }

        public int DoctorId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public virtual Doctor Doctor { get; set; } = null;




    }
}
