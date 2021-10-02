using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MSFramework.AspNetCore.Test.DataModel
{
	public class TestData: ModificationAggregateRoot
	{
		public TestData() : base(ObjectId.GenerateNewId())
		{
		}
	}
}