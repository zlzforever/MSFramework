using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class User : ValueObject
	{
		public Guid Id { get; private set; }

		public string FirstName { get; private set; }

		public string LastName { get; private set; }

		/// <summary>
		/// 所在组
		/// </summary>
		public string GroupName { get; private set; }

		/// <summary>
		/// 所在团队
		/// </summary>
		public string TeamName { get; private set; }

		public User(Guid id, string firstName, string lastName, string groupName, string teamName)
		{
			Id = id;
			FirstName = firstName;
			LastName = lastName;
			GroupName = groupName;
			TeamName = teamName;
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Id;
			yield return FirstName;
			yield return LastName;
			yield return GroupName;
			yield return TeamName;
		}
	}
}