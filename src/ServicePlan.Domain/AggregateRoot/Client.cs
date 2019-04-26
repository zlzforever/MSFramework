using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	/// <summary>
	/// 客户
	/// </summary>
	public class Client : ValueObject
	{
		public Guid Id { get; }
		
		public string Name { get; }

		public string ShortName { get; }

		public Client(Guid id, string name, string shortName)
		{
			Id = id;
			Name = name;
			ShortName = shortName;
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Id;
			yield return Name;
			yield return ShortName;
		}
	}
}