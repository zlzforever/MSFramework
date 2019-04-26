using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class Product : ValueObject
	{
		private Guid Id { get; }

		private string Name { get;}

		private ProductType Type { get; set; }

		private string KeyIdeaAndTopic { get; }

		public Product(Guid id, string name, string keyIdeaAndTopic)
		{
			Id = id;
			Name = name;
			KeyIdeaAndTopic = keyIdeaAndTopic;
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Id;
			yield return Name;
			yield return KeyIdeaAndTopic;
			yield return Type;
		}
	}
}