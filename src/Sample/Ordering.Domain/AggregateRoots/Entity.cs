using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots;

public class Entity1(int id) : CreationAggregateRoot<int>(id);

public class Entity2(ObjectId id) : CreationAggregateRoot(id);
