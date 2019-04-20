using System;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Domain.Events
{
	using MediatR;
	using System.Collections.Generic;

	/// <summary>
	/// Event used when the grace period order is confirmed
	/// </summary>
	public class OrderStatusChangedToAwaitingValidationDomainEvent
		: DomainEventBase<Guid>
	{
		public IEnumerable<OrderItem> OrderItems { get; }

		public OrderStatusChangedToAwaitingValidationDomainEvent(
			IEnumerable<OrderItem> orderItems)
		{
			OrderItems = orderItems;
		}
	}
}