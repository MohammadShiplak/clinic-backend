using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.API.Services
{
    public class DoctorService(ClinicDbContext context) : IDoctorService
    {

        private static int? CalculateYears(DateOnly? start, DateOnly today)
        {
            if (start is null || start > today) return null;
            var years = today.Year - start.Value.Year;
            if (start.Value.AddYears(years) > today) years--;
            return years;
        }
        public async Task<IReadOnlyList<string>> GetSpecializationAsync(CancellationToken ct)=>
            await context.Doctors.AsNoTracking()
            .Where(d=>d.Specialization != null)
            .Select(d=>d.Specialization!)
            .Distinct()
            .OrderBy(s=>s)
            .ToListAsync(ct);

        public async Task<DoctorDetailsDto?> GetDoctorByIdAsync(int id, CancellationToken ct)
        {
            var row = await context.Doctors
                .AsNoTracking()
                .Where(d => d.DoctorId == id)
                .Select(d => new
                {

                    d.DoctorId,
                    d.Person.Name,
                    d.Specialization,
                    d.ProfileImageUrl,
                    d.CareerStarDate,
                    d.Biography,
                    d.ClinicRoom,
                    Quilifications = d.Qualifications
                   .OrderByDescending(q => q.YearAwarded)
                   .Select(q => new DoctorQualificationDto(q.Title, q.Institution, q.YearAwarded))
                   .ToList()

                }).FirstOrDefaultAsync(ct);

            if(row is null) return null;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);


            return new DoctorDetailsDto(

                row.DoctorId,
                row.Name,
                row.Specialization,
                row.ProfileImageUrl,
                CalculateYears(row.CareerStarDate, today),
                row.Biography,
                row.ClinicRoom,
                row.Quilifications);


        }

        private static IOrderedQueryable<Doctor> ApplySorting(
            IQueryable<Doctor> q, DoctorSortBy sortBy, SortDirection direction)
        {
            var desc = direction == SortDirection.Desc;

            return sortBy switch
            {
                DoctorSortBy.Specialization => desc
                    ? q.OrderByDescending(d => d.Specialization).ThenBy(d => d.Person.Name).ThenBy(d => d.DoctorId)
                    : q.OrderBy(d => d.Specialization).ThenBy(d => d.Person.Name).ThenBy(d => d.DoctorId),

                // Experience is derived from the start date:
                // earlier start date = MORE experience. "Desc" (most experienced first) = start date ASC.
                // Doctors with no start date always go last.
                DoctorSortBy.Experience => desc
                    ? q.OrderBy(d => d.CareerStarDate == null).ThenBy(d => d.CareerStarDate).ThenBy(d => d.DoctorId)
                    : q.OrderBy(d => d.CareerStarDate == null).ThenByDescending(d => d.CareerStarDate).ThenBy(d => d.DoctorId),

                _ => desc // Name
                    ? q.OrderByDescending(d => d.Person.Name).ThenBy(d => d.DoctorId)
                    : q.OrderBy(d => d.Person.Name).ThenBy(d => d.DoctorId),
            };
        }
        public async  Task<PagedResult<DoctorListItemDto>> GetDoctorsAsync(DoctorQueryParameters p, CancellationToken ct)
        {
            var query =  context.Doctors.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(p.Search))
                query = query.Where(d => d.Person.Name.Contains(p.Search));

            if (!string.IsNullOrWhiteSpace(p.Specialization))
                query = query.Where(d => d.Specialization == p.Specialization);

            // 1) Count AFTER filters, BEFORE paging
            var totalCount = await query.CountAsync(ct);

            // 2) Sort, 3) page, 4) project: all translated to one SQL query
            var rows = await ApplySorting(query, p.SortBy, p.SortDirection)
                .Skip((p.PageNumber - 1) * p.PageSize)
                .Take(p.PageSize)
                .Select(d => new
                {
                    d.DoctorId,
                    d.Person.Name,
                    d.Specialization,
                    d.ProfileImageUrl,
                    d.CareerStarDate
                })
                .ToListAsync(ct);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var items = rows
                .Select(r => new DoctorListItemDto(
                    r.DoctorId,
                    r.Name,
                    r.Specialization,
                    r.ProfileImageUrl,
                    CalculateYears(r.CareerStarDate, today)))
                .ToList();

            return new PagedResult<DoctorListItemDto>(items, p.PageNumber, p.PageSize, totalCount);
        }
    }
}
