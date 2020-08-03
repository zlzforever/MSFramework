using System;
using MSFramework.Common;
using MSFramework.Domain.AggregateRoot;

namespace MSFramework.AspNetCore.Test.DataModel
{
	public class TestData: ModificationAuditedAggregateRoot
	{
		public TestData() : base(ObjectId.NewId())
		{
		}
	}
}