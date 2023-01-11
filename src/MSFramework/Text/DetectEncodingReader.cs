using System.Linq;
using System.Text;

namespace MicroserviceFramework.Text;

public class DetectEncodingReader
{
    public static string Read(byte[] bytes, params string[] encodingNames)
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

        throw new MicroserviceFrameworkFriendlyException("无法解码文件");
    }

    public static string Read(byte[] bytes)
    {
        return Read(bytes, "UTF-8", "GB2312", "GBK");
    }
}
