using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Text.Json;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Text.Json;
using MicroserviceFramework.Text.Json.Converters;
using MongoDB.Bson;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MSFramework.Tests;

/// <summary>
/// ZZO-187 回归测试：验证 Utf8JsonReader 在跨段 ReadOnlySequence&lt;byte&gt;（HasValueSequence 为 true）场景下，
/// ObjectId / Enumeration 转换器通过 reader.GetString() 正确读取跨段值，
/// 避免旧实现读取 ValueSpan 为空导致枚举解析失败、ObjectId 静默变 Empty 的问题。
/// </summary>
public class StreamingDeserializationRegressionTests
{
    /// <summary>
    /// 待反序列化测试模型：同时包含 ObjectId 与 Enumeration 字段，覆盖两个转换器的跨段读取
    /// </summary>
    private class Obj
    {
        /// <summary>对象标识，ObjectId 类型字段</summary>
        public ObjectId Id { get; set; }

        /// <summary>业务枚举，Enumeration 类型字段</summary>
        public Enum1 Enum { get; set; }
    }

    /// <summary>
    /// 测试用 <see cref="Enumeration"/> 派生类型，静态字段作为可匹配的合法枚举值
    /// </summary>
    private class Enum1(string id, string name) : Enumeration(id, name)
    {
        /// <summary>合法枚举值 Graph</summary>
        public static Enum1 Graph = new Enum1(nameof(Graph), nameof(Graph));

        /// <summary>合法枚举值 Property</summary>
        public static Enum1 Property = new Enum1(nameof(Property), nameof(Property));
    }

    /// <summary>
    /// 跨段场景端到端验证：分段 buffer 中 ObjectId 与 Enumeration 字段反序列化值均正确保留，
    /// 且测试前置断言字符串值 token 确实跨段（HasValueSequence 为 true），确保覆盖修复路径
    /// </summary>
    [Fact]
    public void Deserialize_CrossSegmentBuffer_ObjectIdAndEnumeration_ValuesPreserved()
    {
        // 切分点 20 位于 ObjectId 十六进制串内部，切分点 44 位于 "Graph" 内部，确保两个字符串值均跨段
        var payload = Encoding.UTF8.GetBytes("{\"id\":\"507f1f77bcf86cd799439011\",\"enum\":\"Graph\"}");
        AssertStringValuesSpanSegments(payload, 20, 44);

        var options = TextJsonSerializer.CreateDefaultOptions();
        var reader = new Utf8JsonReader(BuildSegmentedSequence(payload, 20, 44));

        var obj = JsonSerializer.Deserialize<Obj>(ref reader, options);

        Assert.Equal(new ObjectId("507f1f77bcf86cd799439011"), obj.Id);
        Assert.Equal(Enum1.Graph, obj.Enum);
    }

    /// <summary>
    /// 直接调用 ObjectIdJsonConverter.Read：值跨段时返回正确的 ObjectId，而非静默变为 ObjectId.Empty
    /// </summary>
    [Fact]
    public void ObjectIdJsonConverter_Read_CrossSegmentValue_ReturnsExpectedId()
    {
        // 切分点 20 位于 24 位十六进制串内部，使 ObjectId 值跨两段
        var payload = Encoding.UTF8.GetBytes("{\"id\":\"507f1f77bcf86cd799439011\"}");
        var reader = CreateReaderAtStringValue(BuildSegmentedSequence(payload, 20), 0);
        Assert.True(reader.HasValueSequence);

        var converter = new ObjectIdJsonConverter();
        var result = converter.Read(ref reader, typeof(ObjectId), new JsonSerializerOptions());

        Assert.Equal(new ObjectId("507f1f77bcf86cd799439011"), result);
    }

    /// <summary>
    /// 直接调用 EnumerationJsonConverter.Read：值跨段时返回匹配的枚举实例，而非抛出解析异常
    /// </summary>
    [Fact]
    public void EnumerationJsonConverter_Read_CrossSegmentValue_ReturnsMatchingEnum()
    {
        // 切分点 11 位于 "Graph" 内部，使枚举值跨两段
        var payload = Encoding.UTF8.GetBytes("{\"enum\":\"Graph\"}");
        var reader = CreateReaderAtStringValue(BuildSegmentedSequence(payload, 11), 0);
        Assert.True(reader.HasValueSequence);

        var converter = new EnumerationJsonConverter<Enum1>();
        var result = converter.Read(ref reader, typeof(Enum1), new JsonSerializerOptions());

        Assert.Equal(Enum1.Graph, result);
    }

    /// <summary>
    /// 回归：普通单段字符串反序列化，ObjectId 与 Enumeration 值均正确
    /// </summary>
    [Fact]
    public void Deserialize_SingleSegment_ObjectIdAndEnumeration_ValuesPreserved()
    {
        var options = TextJsonSerializer.CreateDefaultOptions();

        var obj = JsonSerializer.Deserialize<Obj>(
            "{\"id\":\"507f1f77bcf86cd799439011\",\"enum\":\"Graph\"}", options);

        Assert.Equal(new ObjectId("507f1f77bcf86cd799439011"), obj.Id);
        Assert.Equal(Enum1.Graph, obj.Enum);
    }

    /// <summary>
    /// 回归：null / 空字符串 / 缺省字段均反序列化为 ObjectId.Empty
    /// </summary>
    [Fact]
    public void Deserialize_NullOrEmptyStringToObjectId_ReturnsEmpty()
    {
        var options = TextJsonSerializer.CreateDefaultOptions();

        Assert.Equal(ObjectId.Empty, JsonSerializer.Deserialize<Obj>("{\"id\":null}", options).Id);
        Assert.Equal(ObjectId.Empty, JsonSerializer.Deserialize<Obj>("{\"id\":\"\"}", options).Id);
        Assert.Equal(ObjectId.Empty, JsonSerializer.Deserialize<Obj>("{}", options).Id);
    }

    /// <summary>
    /// 回归：枚举匹配失败时抛出 InvalidOperationException，行为与修复前一致
    /// </summary>
    [Fact]
    public void Deserialize_UnknownEnumerationValue_ThrowsInvalidOperationException()
    {
        var options = TextJsonSerializer.CreateDefaultOptions();

        Assert.Throws<InvalidOperationException>(
            () => JsonSerializer.Deserialize<Obj>("{\"enum\":\"NotFound\"}", options));
    }

    /// <summary>
    /// 断言载荷经切分后所有字符串值 token 均跨段（HasValueSequence 为 true），
    /// 若任一字符串值未跨段则测试无法覆盖跨段修复场景
    /// </summary>
    /// <param name="payload">JSON UTF-8 字节</param>
    /// <param name="splitIndexes">切分字节索引（升序，位于需要跨段的值内部）</param>
    private static void AssertStringValuesSpanSegments(byte[] payload, params int[] splitIndexes)
    {
        var reader = new Utf8JsonReader(BuildSegmentedSequence(payload, splitIndexes));
        var stringValueCount = 0;
        while (reader.Read())
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                continue;
            }

            stringValueCount++;
            Assert.True(reader.HasValueSequence,
                $"第 {stringValueCount} 个字符串值未跨段，切分点无法覆盖跨段修复场景");
        }

        Assert.True(stringValueCount > 0, "载荷中未找到字符串值 token");
    }

    /// <summary>
    /// 构造跨段只读序列的 Utf8JsonReader，并前进到第 valueIndex 个字符串值 token
    /// </summary>
    /// <param name="sequence">跨段只读序列</param>
    /// <param name="valueIndex">字符串值 token 序号（从 0 开始，属性名为 PropertyName 不计入）</param>
    /// <returns>定位到目标字符串值 token 的读取器</returns>
    /// <exception cref="InvalidOperationException">载荷中不存在对应序号的字符串值 token</exception>
    private static Utf8JsonReader CreateReaderAtStringValue(ReadOnlySequence<byte> sequence, int valueIndex)
    {
        var reader = new Utf8JsonReader(sequence);
        var stringValueCount = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (stringValueCount == valueIndex)
                {
                    return reader;
                }

                stringValueCount++;
            }
        }

        throw new InvalidOperationException($"载荷中不存在第 {valueIndex} 个字符串值 token");
    }

    /// <summary>
    /// 依据切分点将字节数组切成多段，构建跨段的 ReadOnlySequence&lt;byte&gt;
    /// </summary>
    /// <param name="payload">JSON UTF-8 字节</param>
    /// <param name="splitIndexes">切分字节索引（升序，需位于 payload 内部）</param>
    /// <returns>多段只读序列</returns>
    private static ReadOnlySequence<byte> BuildSegmentedSequence(byte[] payload, params int[] splitIndexes)
    {
        var indexes = splitIndexes.Append(payload.Length).ToArray();
        var first = new BufferSegment(payload[..indexes[0]].AsMemory());
        var current = first;
        for (var i = 1; i < indexes.Length; i++)
        {
            current = current.Append(payload[indexes[i - 1]..indexes[i]].AsMemory());
        }

        return new ReadOnlySequence<byte>(first, 0, current, current.Memory.Length);
    }

    /// <summary>
    /// ReadOnlySequence 段节点，通过链式 Append 构造跨段只读序列
    /// </summary>
    private sealed class BufferSegment : ReadOnlySequenceSegment<byte>
    {
        /// <summary>以指定字节内存初始化段节点</summary>
        public BufferSegment(ReadOnlyMemory<byte> memory)
        {
            Memory = memory;
        }

        /// <summary>
        /// 追加下一段字节内存并返回新段节点，RunningIndex 累计前序段长度
        /// </summary>
        /// <param name="memory">待追加的字节内存</param>
        /// <returns>新追加的段节点</returns>
        public BufferSegment Append(ReadOnlyMemory<byte> memory)
        {
            var segment = new BufferSegment(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };
            Next = segment;
            return segment;
        }
    }
}
