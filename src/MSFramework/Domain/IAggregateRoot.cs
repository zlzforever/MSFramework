using System;
using System.Collections.Generic;
using MediatR;

namespace MSFramework.Domain
{
	public interface IAggregateRoot<TAggregateId>
		: IEventSourcingAggregate<TAggregateId> where TAggregateId : IEquatable<TAggregateId>
	{
		TAggregateId Id { get; }
	}
}