using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// Some helper methods for entities.
/// </summary>
internal static class EntityHelper
{
    /// <summary>
    /// 判断实体的主键是否为默认/无效值。
    /// </summary>
    /// <typeparam name="TKey">主键类型</typeparam>
    /// <param name="entity">实体实例</param>
    /// <returns>主键为默认值、非正值（int/long 的 EF Core 附加占位值）或 ObjectId.Empty 时返回 true</returns>
    /// <remarks>
    /// int/long 判断采用值的模式匹配而非 <c>typeof(TKey) == typeof(int) + Convert.ToInt32(object)</c>：
    /// 后者在每次调用时显式装箱（Convert 的 object 重载）+ 重载查找，且位于实体相等性判定的热路径。
    /// 泛型值类型的 <c>is</c> 类型检查编译为 box+isinst，JIT 将其识别为无分配的类型比较
    /// </remarks>
    public static bool HasDefaultId<TKey>(IEntity<TKey> entity) where TKey : IEquatable<TKey>
    {
        if (EqualityComparer<TKey>.Default.Equals(entity.Id, default))
        {
            return true;
        }

        // Workaround for EF Core since it sets int/long to min value when attaching to dbContext：
        // 非正值（含 int.MinValue 的附加占位值）视为无效主键
        if (entity.Id is int intValue)
        {
            return intValue <= 0;
        }

        if (entity.Id is long longValue)
        {
            return longValue <= 0;
        }

        return entity.Id is ObjectId objectId && objectId == ObjectId.Empty;
    }
}
