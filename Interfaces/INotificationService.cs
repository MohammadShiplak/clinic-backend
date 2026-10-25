using ClinicManagementSystem.API.Services;

namespace ClinicManagementSystem.API.Interfaces
{


    public interface INotificationService
    {
        /// Adds notifications for patient and doctor. Does NOT save: the caller commits.
        void AddAppointmentBooked(AppointmentNotificationContext appointment);

        /// Notifies every party except the person who cancelled.
        void AddAppointmentCancelled(AppointmentNotificationContext appointment, int cancelledByPersonId);
    }

}