using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// Some helper methods for entities.
/// </summary>
internal static class EntityHelper
{
    public static bool HasDefaultId<TKey>(IEntity<TKey> entity) where TKey : IEquatable<TKey>
    {
        if (EqualityComparer<TKey>.Default.Equals(entity.Id, default))
        {
            return true;
        }

        //Workaround for EF Core since it sets int/long to min value when attaching to dbContext
        if (typeof(TKey) == typeof(int))
        {
            return Convert.ToInt32(entity.Id) <= 0;
        }

        if (typeof(TKey) == typeof(long))
        {
            return Convert.ToInt64(entity.Id) <= 0;
        }

        return typeof(TKey) == typeof(ObjectId) && entity.Id.Equals(ObjectId.Empty);
    }
}
