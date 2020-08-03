using System;
using MSFramework.Common;
using MSFramework.Domain;

namespace MSFramework.AspNetCore.Test.DataModel
{
	public class TestData: ModificationAggregateRoot
	{
		public TestData() : base(ObjectId.NewId())
		{
		}
	}
}