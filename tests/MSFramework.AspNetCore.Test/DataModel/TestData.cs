using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MSFramework.AspNetCore.Test.DataModel;

public class TestData() : ModificationAggregateRoot(ObjectId.GenerateNewId());
