using System.IO;
using System.Text;
using MicroserviceFramework.Serialization.Newtonsoft;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// Newtonsoft 序列化器流接口回归测试：
/// 验证 <c>Serialize(Stream, TValue)</c> 可用（旧实现抛 NotImplementedException）、
/// <c>Deserialize&lt;T&gt;(Stream)</c> 流式解析可用，且两个方法都不关闭调用方传入的流
/// （所有权归调用方：leaveOpen 契约；旧实现 reader 会在方法结束时关闭调用方的流）。
/// </summary>
public class NewtonsoftJsonSerializerStreamTests
{
    /// <summary>
    /// 待序列化测试模型
    /// </summary>
    public class Payload
    {
        /// <summary>名称</summary>
        public string Name { get; set; }

        /// <summary>数量</summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 流序列化：写出 UTF-8 JSON（默认 CamelCase 契约），且流保持打开可继续使用
    /// </summary>
    [Fact]
    public void SerializeToStream_WritesUtf8Json_AndKeepsStreamOpen()
    {
        using var stream = new MemoryStream();
        var serializer = new NewtonsoftJsonSerializer();

        serializer.Serialize(stream, new Payload { Name = "abc", Count = 3 });

        // Assert：流未被关闭（可继续定位与读取），内容为 UTF-8 JSON
        Assert.True(stream.CanWrite);
        stream.Position = 0;
        var text = new StreamReader(stream, Encoding.UTF8, false, 1024, leaveOpen: true).ReadToEnd();
        Assert.Contains("\"name\":\"abc\"", text);
        Assert.Contains("\"count\":3", text);
    }

    /// <summary>
    /// 流反序列化：正确解析值，且流保持打开（未关闭调用方的流）
    /// </summary>
    [Fact]
    public void DeserializeFromStream_ReadsValue_AndKeepsStreamOpen()
    {
        var bytes = Encoding.UTF8.GetBytes("{\"name\":\"abc\",\"count\":3}");
        using var stream = new MemoryStream(bytes);
        var serializer = new NewtonsoftJsonSerializer();

        // Act
        var result = serializer.Deserialize<Payload>(stream);

        // Assert：值正确；流未被关闭（关闭后再读会抛 ObjectDisposedException）且已读到末尾
        Assert.Equal("abc", result.Name);
        Assert.Equal(3, result.Count);
        Assert.True(stream.CanRead);
        Assert.Equal(-1, stream.ReadByte());
    }

    /// <summary>
    /// 同一内存流上"序列化 → 回退 → 反序列化"完整往返，验证 leaveOpen 后流可复用
    /// </summary>
    [Fact]
    public void RoundTrip_OnSameStream_Works()
    {
        using var stream = new MemoryStream();
        var serializer = new NewtonsoftJsonSerializer();

        serializer.Serialize(stream, new Payload { Name = "round", Count = 9 });
        stream.Seek(0, SeekOrigin.Begin);
        var result = serializer.Deserialize<Payload>(stream);

        Assert.Equal("round", result.Name);
        Assert.Equal(9, result.Count);
    }

    /// <summary>
    /// 额外内容检查回归：流中根对象之后还有尾随对象（"{} {}"）时必须抛
    /// <see cref="JsonReaderException"/>，与字符串重载的 CheckAdditionalContent 行为一致——
    /// 旧实现静默返回第一个对象（行为回归）
    /// </summary>
    [Fact]
    public void DeserializeFromStream_TrailingObject_Throws()
    {
        var serializer = new NewtonsoftJsonSerializer();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{} {}"));

        Assert.Throws<Newtonsoft.Json.JsonReaderException>(
            () => serializer.Deserialize<Payload>(stream));
    }

    /// <summary>
    /// 额外内容检查回归：非法尾随文本同样拒绝（合法对象 + 垃圾字符）
    /// </summary>
    [Fact]
    public void DeserializeFromStream_TrailingIllegalText_Throws()
    {
        var serializer = new NewtonsoftJsonSerializer();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"abc\"} junk"));

        Assert.Throws<Newtonsoft.Json.JsonReaderException>(
            () => serializer.Deserialize<Payload>(stream));
    }

    /// <summary>
    /// 尾随注释尊重 Newtonsoft 语义：与字符串重载一致，
    /// CheckAdditionalContent 默认开启时仍接受根对象之后的注释
    /// </summary>
    [Fact]
    public void DeserializeFromStream_TrailingComment_Accepted()
    {
        var serializer = new NewtonsoftJsonSerializer();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{} /* comment */"));

        var result = serializer.Deserialize<Payload>(stream);

        Assert.NotNull(result);
        // 流保持打开（leaveOpen 契约）
        Assert.True(stream.CanRead);
    }

    /// <summary>
    /// 调用方显式关闭 CheckAdditionalContent 时尊重配置：与字符串重载一致，
    /// 尾随对象不再抛出（手工无条件检查会忽略该配置）
    /// </summary>
    [Fact]
    public void DeserializeFromStream_CheckAdditionalContentDisabled_Honored()
    {
        var serializer = new NewtonsoftJsonSerializer(
            new Newtonsoft.Json.JsonSerializerSettings { CheckAdditionalContent = false });
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{} {}"));

        var result = serializer.Deserialize<Payload>(stream);

        Assert.NotNull(result);
    }

    /// <summary>
    /// 无额外内容的正常流不受检查影响：单个对象照常反序列化，流保持打开
    /// </summary>
    [Fact]
    public void DeserializeFromStream_SingleObject_NoAdditionalContentCheckSideEffect()
    {
        var serializer = new NewtonsoftJsonSerializer();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"abc\",\"count\":3}"));

        var result = serializer.Deserialize<Payload>(stream);

        Assert.Equal("abc", result.Name);
        Assert.Equal(3, result.Count);
        // 流保持打开（leaveOpen 契约）
        Assert.True(stream.CanRead);
    }
}
