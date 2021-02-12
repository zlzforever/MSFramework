using System;
using System.Collections.Generic;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Domain
{
	[Serializable]
	public abstract class AggregateRoot<TKey> :
		EntityBase<TKey>,
		IAggregateRoot<TKey> where TKey : IEquatable<TKey>
	{
		protected AggregateRoot(TKey id) : base(id)
		{
		}
	}
}