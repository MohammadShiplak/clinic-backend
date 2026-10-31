// Services/PatientService.cs
using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.API.Services;

public class PatientService(ClinicDbContext context,TimeProvider clock) : IPatientService
{
    private const int MaxPageSize = 50;

    public async Task<PagedResult<PatientListItemDto>> GetPatientsAsync(
        string? search, bool includeInactive, int pageNumber, int pageSize, CancellationToken ct)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = context.Patients.AsNoTracking();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Person.Name.Contains(term) ||
                p.Person.PhoneNumber.Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var rows = await query
            .OrderBy(p => p.Person.Name)
            .ThenBy(p => p.PatientId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.PatientId,
                p.Person.Name,
                p.Person.PhoneNumber,
                p.Person.Email,
                p.Person.DateOfBirth,
                p.IsActive
            })
            .ToListAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var items = rows
            .Select(r => new PatientListItemDto(
                r.PatientId,
                r.Name,
                r.PhoneNumber,
                r.Email,
                DateMath.WholeYearsBetween(r.DateOfBirth, today),
                r.IsActive))
            .ToList();

        return new PagedResult<PatientListItemDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PatientDetailsDto?> GetPatientAsync(int patientId, CancellationToken ct) =>
        await context.Patients
            .AsNoTracking()
            .Where(p => p.PatientId == patientId)
            .Select(p => new PatientDetailsDto(
                p.PatientId,
                p.Person.Name,
                p.Person.DateOfBirth,
                p.Person.Gender,
                p.Person.PhoneNumber,
                p.Person.Email,
                p.Person.Address,
                p.IsActive))
            .FirstOrDefaultAsync(ct);

    public async Task<CreatePatientResult> CreateAsync(CreatePatientDto dto, CancellationToken ct)
    {
        var name = dto.FullName.Trim();
        var phone = dto.PhoneNumber.Trim();

        var duplicate = await context.Patients.AnyAsync(p =>
            p.Person.Name == name &&
            p.Person.PhoneNumber == phone &&
            p.Person.DateOfBirth == dto.DateOfBirth, ct);

        if (duplicate)
            return new CreatePatientResult(CreatePatientOutcome.Duplicate, null);

        var patient = new Patient
        {
            Person = new Person
            {
                Name = name,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                PhoneNumber = phone,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim()
            }
        };

        context.Patients.Add(patient);
        await context.SaveChangesAsync(ct);

        var created = await GetPatientAsync(patient.PatientId, ct);
        return new CreatePatientResult(CreatePatientOutcome.Created, created);
    }

    public async Task<PatientDetailsDto?> UpdateAsync(int patientId, UpdatePatientDto dto, CancellationToken ct)
    {
        var patient = await context.Patients
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.PatientId == patientId, ct);

        if (patient is null) return null;

        var person = patient.Person;
        person.Name = dto.FullName.Trim();
        person.DateOfBirth = dto.DateOfBirth;
        person.Gender = dto.Gender;
        person.PhoneNumber = dto.PhoneNumber.Trim();
        person.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        person.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();

        await context.SaveChangesAsync(ct);

        return await GetPatientAsync(patientId, ct);
    }

    public Task<bool> DeactivateAsync(int patientId, CancellationToken ct) =>
        SetActiveAsync(patientId, false, ct);

    public Task<bool> ReactivateAsync(int patientId, CancellationToken ct) =>
        SetActiveAsync(patientId, true, ct);

    private async Task<bool> SetActiveAsync(int patientId, bool isActive, CancellationToken ct)
    {
        var rows = await context.Patients
            .Where(p => p.PatientId == patientId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, isActive), ct);

        return rows > 0;
    }
    public async Task<PagedResult<PatientAppointmentDto>?> GetAppointmentHistoryAsync(
    int patientId, PatientAppointmentsQuery q, CancellationToken ct)
    {
        var patientExists = await context.Patients.AnyAsync(p => p.PatientId == patientId, ct);
        if (!patientExists) return null;

        var pageNumber = Math.Max(q.PageNumber, 1);
        var pageSize = Math.Clamp(q.PageSize, 1, MaxPageSize);

        var query = context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId);

        if (q.Status is { } status)
            query = query.Where(a => a.Status == status);

        if (q.From is { } from)
        {
            var start = from.ToDateTime(TimeOnly.MinValue);
            query = query.Where(a => a.AppointmentDateTime >= start);
        }

        if (q.To is { } to)
        {
            var endExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(a => a.AppointmentDateTime < endExclusive);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.AppointmentDateTime)
            .ThenByDescending(a => a.AppointmentId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new PatientAppointmentDto(
                a.AppointmentId,
                a.AppointmentDateTime,
                a.Status,
                a.DoctorId,
                a.Doctor.Person.Name,
                a.Doctor.Specialization,
                a.MedicalRecordId != null,
                a.Payment != null ? a.Payment.AmountPaid : (decimal?)null))
            .ToListAsync(ct);

        return new PagedResult<PatientAppointmentDto>(items, totalCount, pageNumber, pageSize);
    }

   async  Task<DeactivatePatientOutcome> IPatientService.DeactivateAsync(int patientId, CancellationToken ct)
    {
        var exists = await context.Patients.AnyAsync(p => p.PatientId == patientId, ct);
        if (!exists) return DeactivatePatientOutcome.NotFound;

        var now = clock.GetLocalNow().DateTime;

        var hasUpcoming = await context.Appointments.AnyAsync(a =>
            a.PatientId == patientId &&
            a.Status == AppointmentStatus.Scheduled &&
            a.AppointmentDateTime > now, ct);

        if (hasUpcoming) return DeactivatePatientOutcome.HasUpcomingAppointments;

        await SetActiveAsync(patientId, false, ct);
        return DeactivatePatientOutcome.Deactivated;
    }
  
}