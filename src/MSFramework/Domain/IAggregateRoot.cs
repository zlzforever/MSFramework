﻿using System;
using System.Collections.Generic;
using MediatR;

namespace MSFramework.Domain
{
	public interface IAggregateRoot : IAggregateId
	{
		void RegisterDomainEvent(IDomainEvent @event);

		IEnumerable<IDomainEvent> GetDomainEvents();

		void ClearDomainEvents();
	}

	public interface IAggregateRoot<out TAggregateId>
		: IEventSourcingAggregate where TAggregateId : IEquatable<TAggregateId>
	{
		TAggregateId Id { get; }
	}
}