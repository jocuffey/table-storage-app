using System.Globalization;

namespace TableStoragePoc.Extensions;

public abstract class DateTimeExtensions
{
    public static DateTime ParseExact(string s, string format)
    {
        var dateTimeUnspecified = DateTime.ParseExact(s, format, CultureInfo.InvariantCulture);
        return DateTime.SpecifyKind(dateTimeUnspecified, DateTimeKind.Utc);
    }
}