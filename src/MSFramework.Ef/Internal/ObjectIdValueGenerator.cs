using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
///
/// </summary>
public class ObjectIdValueGenerator : ValueGenerator<ObjectId>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public override ObjectId Next(EntityEntry entry)
    {
        return ObjectId.GenerateNewId();
    }

    /// <summary>
    ///
    /// </summary>
    public override bool GeneratesTemporaryValues => false;
}
