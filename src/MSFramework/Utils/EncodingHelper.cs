using System;
using System.Collections.Generic;
using System.Text;

namespace MicroserviceFramework.Utils;

/// <summary>
/// 编码辅助类，提供多编码尝试解码功能
/// </summary>
/// <remarks>
/// GB2312/GBK 等代码页编码在 .NET Core 3.0+ 默认不可用，须注册
/// <see cref="CodePagesEncodingProvider"/> 后使用。本类在类型初始化时自动注册
/// （<see cref="Encoding.RegisterProvider"/> 可重复调用，不抛异常），调用方无需自行注册。
/// </remarks>
public static class EncodingHelper
{
    static EncodingHelper()
    {
        // 代码页提供程序：没有它，Encoding.GetEncoding("GB2312"/"GBK") 会抛
        // ArgumentException（'GB2312' is not a supported encoding name），而非走到
        // 多编码尝试逻辑。样例宿主（Ordering.API/Program.cs）自行注册过，但框架作为类库
        // 不能依赖宿主的注册顺序，这里在类型初始化时兜底注册（幂等）
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// 按候选编码名称解析编码实例；无法解析的候选（未注册提供程序、名称未知）跳过而不中断。
    /// 解析失败抛 <see cref="ArgumentException"/> 而非文档承诺的业务异常，且发生在
    /// <c>try</c> 之外会导致整个解码直接失败——这里将其纳入候选过滤
    /// </summary>
    /// <param name="encodingNames">候选编码名称列表</param>
    /// <returns>可用的编码实例序列（按候选顺序）</returns>
    private static IEnumerable<Encoding> ResolveEncodings(IEnumerable<string> encodingNames)
    {
        foreach (var name in encodingNames)
        {
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(name,
                    new EncoderExceptionFallback(), new DecoderExceptionFallback());
            }
            catch (ArgumentException)
            {
                // 未注册代码页提供程序或编码名称未知：跳过该候选，继续尝试后续编码
                continue;
            }

            yield return encoding;
        }
    }

    /// <summary>
    /// 尝试多种编码解码字节数据，直到成功
    /// </summary>
    /// <param name="bytes">待解码的字节数据</param>
    /// <param name="encodingNames">候选编码名称列表</param>
    /// <returns>解码后的字符串</returns>
    /// <exception cref="MicroserviceFrameworkException">所有候选编码（含无法解析的候选被跳过后）均无法解码时抛出</exception>
    public static string GetString(ReadOnlySpan<byte> bytes, params string[] encodingNames)
    {
        foreach (var encoding in ResolveEncodings(encodingNames))
        {
            try
            {
                return encoding.GetString(bytes);
            }
            catch
            {
                // 该候选编码解码失败（含字节非法触发 DecoderFallbackException），尝试下一个候选
            }
        }

        throw new MicroserviceFrameworkException(
            $"无法用候选编码 [{string.Join(", ", encodingNames)}] 解码数据");
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
    /// <exception cref="MicroserviceFrameworkException">所有候选编码（含无法解析的候选被跳过后）均无法解码时抛出</exception>
    public static string GetString(byte[] bytes, params string[] encodingNames)
    {
        foreach (var encoding in ResolveEncodings(encodingNames))
        {
            try
            {
                return encoding.GetString(bytes);
            }
            catch
            {
                // 该候选编码解码失败（含字节非法触发 DecoderFallbackException），尝试下一个候选
            }
        }

        throw new MicroserviceFrameworkException(
            $"无法用候选编码 [{string.Join(", ", encodingNames)}] 解码数据");
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
