using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.API.Services
{
    public class DoctorService(ClinicDbContext context) : IDoctorService
    {
        public async Task<IReadOnlyList<DoctorListItemDto>> GetDoctorsAsync(string? search, string? specialization, CancellationToken ct)
        {
            var query = context.Doctors.AsNoTracking();

            if(!string.IsNullOrEmpty(search))
                query=query.Where(d=>d.Person.Name.Contains(search));   

            if(!string.IsNullOrEmpty(specialization))
             query=query.Where(s=>s.Specialization ==  specialization);

            var rows = await query
                .OrderBy(d => d.Person.Name)
                .Select(d => new
                {

                    d.DoctorId,
                    d.Person.Name,
                    d.Specialization,
                    d.ProfileImageUrl,
                    d.CareerStarDate,
                   
                    
                })
                .ToListAsync(ct);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);


            return rows.Select(r=> new DoctorListItemDto
            (
                
                r.DoctorId,
                r.Name,
                r.Specialization,
                r.ProfileImageUrl,
               CalculateYears(r.CareerStarDate, today)

                )).ToList();



        }
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
    }
}
