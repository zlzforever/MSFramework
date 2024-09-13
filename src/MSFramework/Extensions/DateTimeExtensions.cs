using System;

namespace MicroserviceFramework.Extensions;

/// <summary>
///
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static long ToUnixTimeSeconds(this DateTime dt)
    {
        var ts = dt - DateTimeOffset.UnixEpoch;
        var timestamp = Convert.ToInt64(ts.TotalSeconds);
        return timestamp;
    }
}
