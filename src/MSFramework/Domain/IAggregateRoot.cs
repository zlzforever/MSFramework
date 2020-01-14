using System;
using System.ComponentModel;
using MSFramework.Domain.Entity;

namespace MSFramework.Domain
{
	/// <summary>
	/// Represents an aggregate root.
	/// </summary>
	public interface IAggregateRoot<out TKey> :
		IEventProvider, IAggregateRoot, IEntity<TKey>
		where TKey : IEquatable<TKey>
	{
	}

	public interface IAggregateRoot : IEntity
	{
	}
}