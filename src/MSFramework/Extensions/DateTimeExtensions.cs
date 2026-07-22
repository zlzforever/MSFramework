using System;

namespace MicroserviceFramework.Extensions;

/// <summary>
/// <see cref="DateTime"/> 扩展方法
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 将 <see cref="DateTime"/> 转为 Unix 时间戳秒数，自动处理 Kind 转换。
    /// </summary>
    /// <param name="dt">本地或 UTC 时间</param>
    /// <returns>Unix 时间戳（秒）</returns>
    public static long ToUnixTimeSeconds(this DateTime dt)
    {
        var utcDt = dt.Kind switch
        {
            DateTimeKind.Local => dt.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
            _ => dt
        };

        var target = new DateTimeOffset(utcDt);
        return target.ToUnixTimeSeconds();
    }
}
