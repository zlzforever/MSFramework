using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// GetBodyTextAsync 回归测试：
/// 请求体在开启缓冲前已被部分消费时必须抛出，不得静默返回残缺数据
/// </summary>
public class HttpContextAccessorExtensionTests
{
    /// <summary>
    /// 不可 seek 的原始请求体包装（模拟 Kestrel 请求流）
    /// </summary>
    private sealed class NonSeekableStream(Stream inner) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            inner.Read(buffer, offset, count);

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken) =>
            await inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    [Fact]
    public async Task GetBodyTextAsync_ReadsFullBodyFromUnconsumedRequestAsync()
    {
        var payload = Encoding.UTF8.GetBytes("hello world");
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Body = new NonSeekableStream(new MemoryStream(payload)),
                    ContentLength = payload.Length
                }
            }
        };

        var text = await accessor.GetBodyTextAsync();

        Assert.Equal("hello world", text);
    }

    [Fact]
    public async Task GetBodyTextAsync_PartiallyConsumedBody_ThrowsInsteadOfReturningTruncatedDataAsync()
    {
        // 模拟模型绑定已消费请求体：底层流已读到末尾，缓冲包装只能覆盖"此后"的内容。
        // ContentLength=11 但回读为 0 字节，必须抛出而非静默返回残缺/空数据
        var payload = Encoding.UTF8.GetBytes("hello world");
        var consumed = new MemoryStream(payload);
        consumed.Position = consumed.Length;

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Body = new NonSeekableStream(consumed),
                    ContentLength = payload.Length
                }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => accessor.GetBodyTextAsync());
    }

    [Fact]
    public async Task GetBodyTextAsync_SeekableBody_IsIdempotentAndRestoresPositionAsync()
    {
        // 已开启缓冲（可 seek）的请求体：重复读取结果一致，且读取后位置归位
        var payload = Encoding.UTF8.GetBytes("hello world");
        var body = new MemoryStream(payload);
        var context = new DefaultHttpContext
        {
            Request =
            {
                Body = body,
                ContentLength = payload.Length
            }
        };
        var accessor = new HttpContextAccessor { HttpContext = context };

        Assert.Equal("hello world", await accessor.GetBodyTextAsync());
        Assert.Equal("hello world", await accessor.GetBodyTextAsync());
        Assert.Equal(0, body.Position);
    }

    [Fact]
    public async Task GetBodyTextAsync_WithoutHttpContext_ReturnsNullAsync()
    {
        var accessor = new HttpContextAccessor();
        Assert.Null(await accessor.GetBodyTextAsync());
    }
}
