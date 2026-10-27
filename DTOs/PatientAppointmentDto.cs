// DTOs/PatientAppointmentDto.cs
using ClinicManagementSystem.API.Entities;

namespace ClinicManagementSystem.API.DTOs;

public record PatientAppointmentDto(
    int AppointmentId,
    DateTime AppointmentDateTime,
    AppointmentStatus Status,
    int DoctorId,
    string DoctorName,
    string? Specialization,
    bool HasMedicalRecord,
    decimal? AmountPaid);