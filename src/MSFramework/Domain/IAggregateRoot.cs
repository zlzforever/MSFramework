using System;

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
		Guid Id { get; }

		bool IsTransient();
	}
}