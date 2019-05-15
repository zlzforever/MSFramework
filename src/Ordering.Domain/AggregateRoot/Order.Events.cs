using System;
using System.Collections.Generic;
using MSFramework.Domain;
using MSFramework.EventBus;

namespace Ordering.Domain.AggregateRoot
{
	public class OrderDeletedEvent : Event
	{
	}

	public class OrderCreatedEvent : Event
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

	public class OrderAddressChangedEvent : Event
	{
		public Address NewOrderAddress { get; }

		public OrderAddressChangedEvent(Address newOrderAddress)
		{
			NewOrderAddress = newOrderAddress;
		}
	}
}