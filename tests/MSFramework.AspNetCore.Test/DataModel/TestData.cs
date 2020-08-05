using MSFramework.Domain;
using MSFramework.Shared;

namespace MSFramework.AspNetCore.Test.DataModel
{
	public class TestData: ModificationAggregateRoot
	{
		public TestData() : base(ObjectId.NewId())
		{
		}
	}
}