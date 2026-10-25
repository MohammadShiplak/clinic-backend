using static ClinicManagementSystem.API.DTOs.DoctorAvailabilityDtos;

namespace ClinicManagementSystem.API
{
    public static class ScheduleValidator
    {
        public static string? Validate(DoctorScheduleDto s)
        {
            foreach (var r in s.WorkingHours.Concat(s.Breaks))
                if (r.EndTime <= r.StartTime)
                    return $"{r.DayOfWeek}: end time must be after start time.";

            foreach (var day in s.WorkingHours.GroupBy(w => w.DayOfWeek))
                if (HasOverlap(day)) return $"Working hours overlap on {day.Key}.";

            foreach (var day in s.Breaks.GroupBy(b => b.DayOfWeek))
                if (HasOverlap(day)) return $"Breaks overlap on {day.Key}.";

            foreach (var b in s.Breaks)
            {
                var inside = s.WorkingHours.Any(w =>
                    w.DayOfWeek == b.DayOfWeek && b.StartTime >= w.StartTime && b.EndTime <= w.EndTime);
                if (!inside) return $"A break on {b.DayOfWeek} must be inside working hours.";
            }
            return null;
        }

        private static bool HasOverlap(IEnumerable<TimeRangeDto> ranges)
        {
            var sorted = ranges.OrderBy(r => r.StartTime).ToList();
            for (var i = 1; i < sorted.Count; i++)
                if (sorted[i].StartTime < sorted[i - 1].EndTime) return true;
            return false;
        }
    }
}
