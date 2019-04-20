using System;
using MediatR;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Domain.Events
{
	public class OrderShippedDomainEvent : DomainEventBase<Guid>
	{
		public Order Order { get; }

		public OrderShippedDomainEvent(Order order)
		{
			Order = order;
		}
	}
}