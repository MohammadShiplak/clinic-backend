using ClinicManagementSystem.API.Entities;

namespace ClinicManagementSystem.API.Services;

public record AppointmentNotificationContext(
    Appointment Appointment,
    int PatientPersonId,
    string PatientName,
    int DoctorPersonId,
    string DoctorName);