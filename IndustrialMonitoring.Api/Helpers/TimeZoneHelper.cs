namespace IndustrialMonitoring.Api.Helpers;

public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo RomeTimeZone = GetRomeTimeZone();

    private static TimeZoneInfo GetRomeTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
    }

    public static DateTime UtcToRome(DateTime utcDateTime)
    {
        var safeUtc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(safeUtc, RomeTimeZone);
    }
}