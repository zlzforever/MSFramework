using System;

namespace MicroserviceFramework.Extensions;

/// <summary>
/// <see cref="DateTime"/> 扩展方法
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 将 <see cref="DateTime"/> 转为 Unix 时间戳秒数，自动处理 Kind 转换。
    /// <para>
    /// <see cref="DateTimeKind.Unspecified"/>（如 SQL Server datetime2 读回的值）按本地墙钟时间处理，
    /// 而非按 UTC，避免库内存储的本地时间被序列化后整体偏移（如北京时间偏移 8 小时）。
    /// </para>
    /// </summary>
    /// <param name="dt">本地或 UTC 时间</param>
    /// <returns>Unix 时间戳（秒）</returns>
    public static long ToUnixTimeSeconds(this DateTime dt)
    {
        var utcDt = dt.Kind switch
        {
            DateTimeKind.Local => dt.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime(),
            _ => dt
        };

        var target = new DateTimeOffset(utcDt);
        return target.ToUnixTimeSeconds();
    }
}
