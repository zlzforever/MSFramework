using System;
using System.IO;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
/// EF Core 专用 JSON ValueComparer。对象属性按名称规范化，数组顺序保持不变。
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
        return CanonicalJson.Equals(left, right);
    }

    /// <summary>
    /// 基于序列化内容计算哈希码，保证与Equals逻辑一致
    /// </summary>
    private static int ComputeContentHashCode(T instance)
    {
        return CanonicalJson.GetHashCode(instance);
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
