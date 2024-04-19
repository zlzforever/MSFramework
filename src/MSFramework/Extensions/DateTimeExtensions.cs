using System;

namespace MicroserviceFramework.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimeSeconds(this DateTime dt)
    {
        var ts = dt - DateTimeOffset.UnixEpoch;
        var timestamp = Convert.ToInt64(ts.TotalSeconds);
        return timestamp;
    }
}
