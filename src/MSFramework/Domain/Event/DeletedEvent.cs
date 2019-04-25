using System;

namespace MSFramework.Domain.Event
{
	public class DeletedEvent<TAggregateRoot, TAggregateRootId> : AggregateEventBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
	}
}