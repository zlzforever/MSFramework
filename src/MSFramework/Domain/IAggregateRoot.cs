using System;
using System.ComponentModel;

namespace MSFramework.Domain
{
	/// <summary>
	/// Represents an aggregate root.
	/// </summary>
	public interface IAggregateRoot : IEventProvider
	{
		/// <summary>
		/// the id as string of the aggregate root.
		/// </summary>
		/// <returns></returns>
		[DisplayName("唯一标识")]
		Guid Id { get; }

		bool IsTransient();
	}
}