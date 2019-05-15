using System;
using System.Collections.Generic;
using MSFramework.EventBus;

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

		int Version { get; }
	}
}