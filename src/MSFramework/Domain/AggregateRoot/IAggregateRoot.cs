using MSFramework.Domain.Entity;

namespace MSFramework.Domain.AggregateRoot
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
	}
}