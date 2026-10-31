namespace ClinicManagementSystem.API.DTOs
{
    public record DashboardDto(
        DoctorStatsDto Doctors,
        AppointmentStatsDto Appointments,
        IReadOnlyList<DailyBookingDto> DailyBookings
    );

    public record DoctorStatsDto(
        int TotalDoctors,
        IReadOnlyList<SpecializationCountDto> BySpecialization
    );

    public record SpecializationCountDto(string Specialization, int Count);

    public record AppointmentStatsDto(
        int Total,
        int Scheduled,
        int Completed,
        int Cancelled,
        int NoShow
    );

    public record DailyBookingDto(DateOnly Date, int Count);
}