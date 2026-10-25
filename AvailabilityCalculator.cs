using ClinicManagementSystem.API.Entities;

namespace ClinicManagementSystem.API
{
    public static class AvailabilityCalculator
    {
        public static IReadOnlyList<DateTime> GetFreeSlots(
            DateOnly date, TimeSpan slotLength,
            IReadOnlyList<DoctorWorkingHour> hours,
            IReadOnlyList<DoctorBreak> breaks,
            IReadOnlyList<DoctorTimeOff> timeOffs,
            IEnumerable<DateTime> bookedStarts)
        {
            var weekday = date.DayOfWeek;
            var dayBreaks = breaks.Where(b => b.DayOfWeek == weekday).ToList();
            var booked = bookedStarts.ToHashSet();
            var slots = new List<DateTime>();

            foreach (var wh in hours.Where(h => h.DayOfWeek == weekday).OrderBy(h => h.StartTime))
            {
                var end = date.ToDateTime(wh.EndTime);
                for (var s = date.ToDateTime(wh.StartTime); s + slotLength <= end; s += slotLength)
                {
                    var e = s + slotLength;
                    var blocked =
                        dayBreaks.Any(b => Overlaps(s, e, date.ToDateTime(b.StartTime), date.ToDateTime(b.EndTime)))
                        || timeOffs.Any(t => Overlaps(s, e, t.StartDateTime, t.EndDateTime))
                        || booked.Contains(s);
                    if (!blocked) slots.Add(s);
                }
            }
            return slots;
        }

        // Half-open intervals [start, end): back-to-back ranges do NOT overlap.
        private static bool Overlaps(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd) =>
            aStart < bEnd && bStart < aEnd;
    }
}
