using System.Text;
using MicroserviceFramework;
using MicroserviceFramework.Utils;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// EncodingHelper 回归测试：
/// GB2312/GBK 等代码页编码须在注册 CodePagesEncodingProvider 后可用，
/// 框架在类型初始化时自动注册（修复前依赖宿主注册，未注册直接抛 ArgumentException）
/// </summary>
public class EncodingHelperTests
{
    [Fact]
    public void GetString_DefaultEncodings_DecodesGb2312()
    {
        // 编码实例自身在注册提供程序后可解析；测试进程未提前注册也不受影响
        var gb2312Bytes = Encoding.GetEncoding("GB2312").GetBytes("测试数据");
        var result = EncodingHelper.GetString(gb2312Bytes);
        Assert.Equal("测试数据", result);
    }

    [Fact]
    public void GetString_UnsupportedCandidateName_IsSkipped()
    {
        // 候选里混入无法解析的编码名（未知名称）：跳过该候选而不是整体失败
        var bytes = Encoding.UTF8.GetBytes("hello");
        var result = EncodingHelper.GetString(bytes, "not-a-real-encoding", "UTF-8");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void GetString_AllCandidatesFail_ThrowsMicroserviceFrameworkException()
    {
        // UTF-8 候选带 DecoderExceptionFallback：非法字节触发失败后再试 GB2312，
        // 全部失败时抛框架业务异常
        var invalidBytes = new byte[] { 0xFF, 0xFE, 0x00 };
        Assert.Throws<MicroserviceFrameworkException>(
            () => EncodingHelper.GetString(invalidBytes, "UTF-8", "GB2312"));
    }
}
