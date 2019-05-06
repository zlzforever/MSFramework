using System.Collections.Generic;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	/// <summary>
	/// Represents an aggregate root.
	/// </summary>
	public interface IAggregateRoot
	{
		/// <summary>
		/// the id as string of the aggregate root.
		/// </summary>
		/// <returns></returns>
		string GetId();

		int Version { get; }

		IEnumerable<IAggregateRootChangedEvent> GetChanges();

		void ClearChanges();
		
		IReadOnlyCollection<IEvent> DomainEvents { get; }
		
		void ClearDomainEvents();

		void LoadFromHistory(params IAggregateRootChangedEvent[] histories);
	}
}