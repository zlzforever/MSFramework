using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
/// EF Core ObjectId 值生成器，自动生成新的 ObjectId
/// </summary>
public class ObjectIdValueGenerator : ValueGenerator<ObjectId>
{
    /// <summary>
    /// 生成新的 ObjectId 值
    /// </summary>
    /// <param name="entry">实体跟踪条目</param>
    /// <returns>新生成的 ObjectId</returns>
    public override ObjectId Next(EntityEntry entry)
    {
        return ObjectId.GenerateNewId();
    }

    /// <summary>
    /// 获取一个值，该值指示生成的值是否为临时值
    /// </summary>
    public override bool GeneratesTemporaryValues => false;
}
