namespace MicroserviceFramework.Utils;

/// <summary>
///
/// </summary>
public static class HttpUtils
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode is >= 200 and <= 299;
    }
}
