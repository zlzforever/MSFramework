using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;

namespace MicroserviceFramework.Ef.Internal;

internal class ObjectIdValueGenerator : ValueGenerator<ObjectId>
{
    public override ObjectId Next(EntityEntry entry)
    {
        return ObjectId.GenerateNewId();
    }

    public override bool GeneratesTemporaryValues => false;
}
