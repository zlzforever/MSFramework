using System;
using System.IO;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
/// EF Core 专用JSON字节流式对比 ValueComparer
/// 逻辑：序列化对象为JSON流，逐字节对比，不一致即判定修改；接受List顺序、字典乱序导致的误判
/// </summary>
public class JsonStreamValueComparer<T> : ValueComparer<T>
{
    /// <summary>
    /// 初始化一个新的 <see cref="JsonStreamValueComparer{T}"/> 实例。
    /// </summary>
    public JsonStreamValueComparer()
        : base(
            BuildEqualsExpression(),
            BuildHashCodeExpression(),
            BuildSnapshotExpression()
        )
    {
    }

    private static Expression<Func<T, T, bool>> BuildEqualsExpression()
    {
        return (left, right) => ContentEquals(left, right);
    }

    private static Expression<Func<T, int>> BuildHashCodeExpression()
    {
        return instance => ComputeContentHashCode(instance);
    }

    private static Expression<Func<T, T>> BuildSnapshotExpression()
    {
        return instance => CreateDeepSnapshot(instance);
    }

    /// <summary>
    /// 基于序列化字节流的内容相等判断
    /// </summary>
    private static bool ContentEquals(T left, T right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;

        using var msLeft = new MemoryStream();
        using var msRight = new MemoryStream();

        Defaults.JsonSerializer.Serialize(msLeft, left);
        Defaults.JsonSerializer.Serialize(msRight, right);

        // 全版本兼容的字节Span对比，.NET Standard 2.1+ / .NET Core 2.1+ 支持
        return msLeft.GetBuffer().AsSpan(0, (int)msLeft.Length)
            .SequenceEqual(msRight.GetBuffer().AsSpan(0, (int)msRight.Length));
    }

    /// <summary>
    /// 基于序列化内容计算哈希码，保证与Equals逻辑一致
    /// </summary>
    private static int ComputeContentHashCode(T instance)
    {
        if (instance is null) return 0;

        using var ms = new MemoryStream();
        Defaults.JsonSerializer.Serialize(ms, instance);

        var buffer = ms.GetBuffer();
        var length = (int)ms.Length;

        unchecked
        {
            var hash = 17;
            for (var i = 0; i < length; i++)
            {
                hash = hash * 31 + buffer[i];
            }

            return hash;
        }
    }

    /// <summary>
    /// 序列化+反序列化实现深拷贝快照，保证EF能检测到内部属性变更
    /// </summary>
    private static T CreateDeepSnapshot(T instance)
    {
        if (instance is null) return default;

        using var ms = new MemoryStream(1024 * 2);
        Defaults.JsonSerializer.Serialize(ms, instance);
        ms.Position = 0;
        return Defaults.JsonSerializer.Deserialize<T>(ms);
    }
}
