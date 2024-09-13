using System.Net;
using System.Net.Sockets;

namespace MicroserviceFramework.Extensions;

/// <summary>
///
/// </summary>
public static class IPAddressExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
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
