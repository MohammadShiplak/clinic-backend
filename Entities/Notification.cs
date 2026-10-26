namespace ClinicManagementSystem.API.Entities
{
    public class Notification
    {

        public int NotificationId { get; set; }

        public int PersonId { get; set; }          // the recipient (patient OR doctor)
        public int? AppointmentId { get; set; }    // what it's about (nullable: future non-appointment notices)

        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ReadAtUtc { get; set; }

        public Person Person { get; set; } = null!;
        public Appointment? Appointment { get; set; }

        public EmailDeliveryStatus EmailStatus { get; set; }
        public int EmailAttempts { get; set; }
        public DateTime? EmailSentAtUtc { get; set; }
        public string? EmailLastError { get; set; }



    }

    public enum NotificationType : byte
    {
        AppointmentBooked = 1,
        AppointmentCancelled = 2,
        AppointmentReminder = 3

    }
    public enum EmailDeliveryStatus : byte { None = 0, Pending = 1, Sent = 2, Failed = 3 }
}
