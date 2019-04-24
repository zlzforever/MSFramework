using System;
using System.Collections.Generic;
using MSFramework.Common;
using MSFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoot
{
	public class OrderDeletedEvent : AggregateEventBase
	{
	}
	
	public class OrderCreatedEvent : AggregateEventBase
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
	
	public class OrderAddressChangedEvent : AggregateEventBase
	{
		public Address NewOrderAddress { get; }

		public OrderAddressChangedEvent(Address newOrderAddress)
		{
			NewOrderAddress = newOrderAddress;
		}
	}
}