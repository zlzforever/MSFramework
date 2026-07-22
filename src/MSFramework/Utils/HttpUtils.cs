namespace MicroserviceFramework.Utils;

/// <summary>
/// HTTP 工具类
/// </summary>
public static class HttpUtils
{
    /// <summary>
    /// 判断 HTTP 状态码是否表示成功（200-299）
    /// </summary>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <returns>true 表示成功状态码</returns>
    public static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode is >= 200 and <= 299;
    }
}
