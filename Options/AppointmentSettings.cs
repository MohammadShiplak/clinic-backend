namespace ClinicManagementSystem.API.Options
{
    public class AppointmentSettings
    {

        public const string SectionName = "Appointments";

        public TimeOnly WorkdayStart { get; init; } = new (9,0);
        public TimeOnly WorkdayEnd { get; init;}=new (17,0);
        public int SlotMinute { get; init; } = 30;

        public string TimeZoneId { get; init; } = "Asia/Amman";





    }
}
