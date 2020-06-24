using System;
using MSFramework.Domain.Entity;

namespace MSFramework.Domain.AggregateRoot
{
	[Serializable]
	public abstract class AggregateRootBase : AggregateRootBase<Guid>
	{
	}

	[Serializable]
	public abstract class AggregateRootBase<TKey> :
		EntityBase<TKey>,
		IAggregateRoot<TKey>
		where TKey : IEquatable<TKey>
	{


		protected AggregateRootBase()
		{
		}

		protected AggregateRootBase(TKey id) : base(id)
		{
		}
	}
}