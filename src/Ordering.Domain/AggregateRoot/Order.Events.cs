using System;
using System.Collections.Generic;
using MSFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoot
{
	public class OrderDeletedEvent : DomainEvent<Order, Guid>
	{
	}

	public class OrderCreatedEvent : DomainEvent<Order, Guid>
	{
		public string Description { get; }

		public string UserId { get; }

		public Address Address { get; }

		public List<OrderItem> OrderItems { get; }

		public OrderCreatedEvent(
			string userId,
			Address address,
			string description,
			List<OrderItem> orderItems
		)
		{
			Description = description;
			Address = address;
			UserId = userId;
			OrderItems = orderItems;
		}
	}

	public class OrderAddressChangedEvent : DomainEvent<Order, Guid>
	{
		public Address NewOrderAddress { get; }

		public OrderAddressChangedEvent(Address newOrderAddress)
		{
			NewOrderAddress = newOrderAddress;
		}
	}
}