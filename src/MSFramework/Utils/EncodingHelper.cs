using System;
using System.Linq;
using System.Text;

namespace MicroserviceFramework.Utils;

public class EncodingHelper
{
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

    public static string GetString(ReadOnlySpan<byte> bytes)
    {
        return GetString(bytes, "UTF-8", "GB2312", "GBK");
    }

    public static string GetString(byte[] bytes)
    {
        return GetString(bytes, "UTF-8", "GB2312", "GBK");
    }
}
