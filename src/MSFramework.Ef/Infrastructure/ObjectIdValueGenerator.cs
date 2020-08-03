using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MSFramework.Common;

namespace MSFramework.Ef.Infrastructure
{
	public class ObjectIdValueGenerator : ValueGenerator<ObjectId>
	{
		public override ObjectId Next(EntityEntry entry)
		{
			return ObjectId.NewId();
		}

		public override bool GeneratesTemporaryValues => false;
	}
}