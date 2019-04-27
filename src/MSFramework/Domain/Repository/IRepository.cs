using System;

namespace MSFramework.Domain.Repository
{
	/// <summary>
	/// This interface must be implemented by all repositories to identify them by convention.
	/// Implement generic version instead of this one.
	/// </summary>
	public interface IRepository
	{
	}

	public interface IRepository<TAggregateRoot, in TAggregateRootId> : IRepository
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
	}
}