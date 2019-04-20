using System;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Domain.Events
{
	using MediatR;
	using System.Collections.Generic;

	/// <summary>
	/// Event used when the order is paid
	/// </summary>
	public class OrderStatusChangedToPaidDomainEvent
		: DomainEventBase<Guid>
	{
		public IEnumerable<OrderItem> OrderItems { get; }

		public OrderStatusChangedToPaidDomainEvent(IEnumerable<OrderItem> orderItems)
		{
			OrderItems = orderItems;
		}
	}
}