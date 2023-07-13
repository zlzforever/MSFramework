using System;

namespace MicroserviceFramework.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimeSeconds(this DateTime dt)
    {
        return ((DateTimeOffset)dt).ToUnixTimeSeconds();
    }
}
