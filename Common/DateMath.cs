// Common/DateMath.cs
namespace ClinicManagementSystem.API.Common;

public static class DateMath
{
    public static int? WholeYearsBetween(DateOnly? start, DateOnly end)
    {
        if (start is null || start > end) return null;

        var years = end.Year - start.Value.Year;
        if (start.Value.AddYears(years) > end) years--;
        return years;
    }
}