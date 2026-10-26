using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using static ClinicManagementSystem.API.Entities.Notification;

namespace ClinicManagementSystem.API.Services;

public class NotificationService(ClinicDbContext context, TimeProvider clock) : INotificationService
{
    public void AddAppointmentBooked(AppointmentNotificationContext c)
    {
        var when = Format(c.Appointment.AppointmentDateTime);

        Add(c.PatientPersonId, c.Appointment, NotificationType.AppointmentBooked,
            "Appointment confirmed",
            $"Your appointment with Dr. {c.DoctorName} is booked for {when}.");

        Add(c.DoctorPersonId, c.Appointment, NotificationType.AppointmentBooked,
            "New appointment",
            $"{c.PatientName} booked an appointment with you for {when}.");
    }

    public void AddAppointmentCancelled(AppointmentNotificationContext c, int cancelledByPersonId)
    {
        var when = Format(c.Appointment.AppointmentDateTime);

        if (c.PatientPersonId != cancelledByPersonId)
            Add(c.PatientPersonId, c.Appointment, NotificationType.AppointmentCancelled,
                "Appointment cancelled",
                $"Your appointment with Dr. {c.DoctorName} on {when} was cancelled.");

        if (c.DoctorPersonId != cancelledByPersonId)
            Add(c.DoctorPersonId, c.Appointment, NotificationType.AppointmentCancelled,
                "Appointment cancelled",
                $"{c.PatientName} cancelled the appointment on {when}.");
    }

    private void Add(int personId, Appointment appointment, NotificationType type, string title, string message)
        => context.Notifications.Add(new Notification
        {
            PersonId = personId,
            Appointment = appointment,   // navigation, NOT AppointmentId: EF fills the FK on save
            Type = type,
            Title = title,
            Message = message,
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
            EmailStatus = EmailDeliveryStatus.Pending,
        });

    private static string Format(DateTime dt) => dt.ToString("ddd, dd MMM yyyy 'at' HH:mm");

    public void AddAppointmentReminder(AppointmentNotificationContext c)
    {
        var when = Format(c.Appointment.AppointmentDateTime);

        Add(c.PatientPersonId, c.Appointment, NotificationType.AppointmentReminder,
            "Appointment reminder",
            $"Reminder: you have an appointment with Dr. {c.DoctorName} on {when}.");

        // One Person can hold both roles (your test data does!). The unique index allows only
        // ONE reminder per (appointment, person), so don't create a second row for the same person.
        if (c.DoctorPersonId != c.PatientPersonId)
            Add(c.DoctorPersonId, c.Appointment, NotificationType.AppointmentReminder,
                "Appointment reminder",
                $"Reminder: you have an appointment with {c.PatientName} on {when}.");
    }
}