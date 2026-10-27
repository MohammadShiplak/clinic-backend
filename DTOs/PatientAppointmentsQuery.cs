// DTOs/PatientAppointmentsQuery.cs
using System.ComponentModel.DataAnnotations;
using ClinicManagementSystem.API.Entities;

namespace ClinicManagementSystem.API.DTOs;

public record PatientAppointmentsQuery(
    AppointmentStatus? Status,
    DateOnly? From,
    DateOnly? To,
    int PageNumber = 1,
    int PageSize = 10) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From is { } from && To is { } to && from > to)
        {
            yield return new ValidationResult(
                "'From' must be on or before 'To'.",
                [nameof(From), nameof(To)]);
        }
    }
}