using System;
using System.Linq;
using System.Text;

namespace MicroserviceFramework.Utils;

/// <summary>
/// 编码辅助类，提供多编码尝试解码功能
/// </summary>
public static class EncodingHelper
{
    /// <summary>
    /// 尝试多种编码解码字节数据，直到成功
    /// </summary>
    /// <param name="bytes">待解码的字节数据</param>
    /// <param name="encodingNames">候选编码名称列表</param>
    /// <returns>解码后的字符串</returns>
    /// <exception cref="MicroserviceFrameworkException">所有编码均无法解码时抛出</exception>
    public static string GetString(ReadOnlySpan<byte> bytes, params string[] encodingNames)
    {
        var encodings = encodingNames.Select(x =>
            Encoding.GetEncoding(x, new EncoderExceptionFallback(), new DecoderExceptionFallback()));
        foreach (var encoding in encodings)
        {
            try
            {
                return encoding.GetString(bytes);
            }
            catch
            {
                //
            }
        }

        throw new MicroserviceFrameworkException("无法解码文件");
    }

    /// <summary>
    /// 使用默认编码（UTF-8/GB2312/GBK）解码字节数据
    /// </summary>
    /// <param name="bytes">待解码的字节数据</param>
    /// <returns>解码后的字符串</returns>
    public static string GetString(ReadOnlySpan<byte> bytes)
    {
        return GetString(bytes, "UTF-8", "GB2312", "GBK");
    }


    /// <summary>
    /// 尝试多种编码解码字节数组，直到成功
    /// </summary>
    /// <param name="bytes">待解码的字节数组</param>
    /// <param name="encodingNames">候选编码名称列表</param>
    /// <returns>解码后的字符串</returns>
    /// <exception cref="MicroserviceFrameworkException">所有编码均无法解码时抛出</exception>
    public static string GetString(byte[] bytes, params string[] encodingNames)
    {
        var encodings = encodingNames.Select(x =>
            Encoding.GetEncoding(x, new EncoderExceptionFallback(), new DecoderExceptionFallback()));
        foreach (var encoding in encodings)
        {
            try
            {
                return encoding.GetString(bytes);
            }
            catch
            {
                //
            }
        }

        throw new MicroserviceFrameworkException("无法解码文件");
    }

    /// <summary>
    /// 使用默认编码（UTF-8/GB2312/GBK）解码字节数组
    /// </summary>
    /// <param name="bytes">待解码的字节数组</param>
    /// <returns>解码后的字符串</returns>
    public static string GetString(byte[] bytes)
    {
        return GetString(bytes, "UTF-8", "GB2312", "GBK");
    }
}
