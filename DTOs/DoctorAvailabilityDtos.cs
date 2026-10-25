namespace ClinicManagementSystem.API.DTOs
{
    public class DoctorAvailabilityDtos
    {
        public record TimeRangeDto(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

        public record DoctorScheduleDto(
            IReadOnlyList<TimeRangeDto> WorkingHours,
            IReadOnlyList<TimeRangeDto> Breaks);

        public record CreateTimeOffDto(DateTime StartDateTime, DateTime EndDateTime, string? Reason);

        public record TimeOffDto(int DoctorTimeOffId, DateTime StartDateTime, DateTime EndDateTime, string? Reason);

        public enum ResultStatus { Ok, NotFound, Invalid, Conflict }

        public record Result<T>(ResultStatus Status, T? Value = default, string? Error = null)
        {
            public static Result<T> Ok(T value) => new(ResultStatus.Ok, value);
            public static Result<T> Fail(ResultStatus status, string error) => new(status, default, error);
        }
    }
}
