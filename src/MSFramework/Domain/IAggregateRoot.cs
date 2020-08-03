using System.Collections.Generic;
using MSFramework.Domain.Events;

namespace MSFramework.Domain
{
	/// <summary>
	/// Represents an aggregate root.
	/// </summary>
	public interface IAggregateRoot<TKey> :
		IAggregateRoot,
		IEntity<TKey>
	{
	}

	/// <summary>
	/// Defines an aggregate root. It's primary key may not be "Id" or it may have a composite primary key.
	/// Use <see cref="IAggregateRoot{TKey}"/> where possible for better integration to repositories and other structures in the framework.
	/// </summary>
	public interface IAggregateRoot : IEntity
	{
		IReadOnlyCollection<IEvent> GetDomainEvents();

		void AddDomainEvent(IEvent @event);

		void RemoveDomainEvent(IEvent @event);

		void ClearDomainEvents();
	}
}