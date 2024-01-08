using System.Net;

namespace MicroserviceFramework.Utils;

public static class HttpUtil
{
    public static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode is >= 200 and <= 299;
    }
}
