using System.Net;
using System.Net.Sockets;

namespace MicroserviceFramework.Extensions;

/// <summary>
/// <see cref="IPAddress"/> 扩展方法
/// </summary>
public static class IPAddressExtensions
{
    /// <summary>
    /// 判断 IP 地址是否为内网地址（10.x, 172.16-31.x, 192.168.x 或回环地址）
    /// </summary>
    /// <param name="address">待判断的 IP 地址</param>
    /// <returns>true 表示内网地址</returns>
    public static bool IsPrivate(this IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        if (address.AddressFamily != AddressFamily.InterNetwork)
        {
            return false;
        }

        var ipBytes = address.GetAddressBytes();
        var one = ipBytes[0];

        // 10.X.X.X
        if (one == 10)
        {
            return true;
        }

        switch (one)
        {
            // 172.16-31.X.X
            case 172 when ipBytes[1] is >= 16 and <= 31:
            // 192.168.X.X
            case 192 when ipBytes[1] == 168:
                return true;
            default:
                return false;
        }
    }
}
