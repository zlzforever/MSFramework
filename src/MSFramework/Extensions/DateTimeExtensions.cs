using System;

namespace MicroserviceFramework.Extensions;

public static class DateTimeExtensions
{
    public static DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long ToUnixTimeSeconds(this DateTime dt)
    {
        var ts = dt - Epoch;
        var timestamp = Convert.ToInt64(ts.TotalSeconds);
        return timestamp;
    }
}
