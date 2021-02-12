using System;

namespace MicroserviceFramework.Domain
{
	/// <summary>
	/// Represents an aggregate root.
	/// </summary>
	public interface IAggregateRoot<out TKey> :
		IEntity<TKey> where TKey : IEquatable<TKey>
	{
	}
}