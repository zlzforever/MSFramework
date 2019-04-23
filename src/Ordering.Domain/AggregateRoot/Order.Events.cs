using System;
using System.Collections.Generic;
using MSFramework.Common;
using MSFramework.Domain.Event;

namespace Ordering.Domain.AggregateRoot
{
	public class OrderDeletedEvent : AggregateEventBase<Guid>
	{
		public OrderDeletedEvent(Guid aggregateId, long version)
			:base(aggregateId, version)
		{
		}
	}
	
	public class OrderCreatedEvent : AggregateEventBase<Guid>
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
		) : base(CombGuid.NewGuid(), 0)
		{
			Description = description;
			Address = address;
			UserId = userId;
			OrderItems = orderItems;
		}
	}
	
	public class OrderAddressChangedEvent : AggregateEventBase<Guid>
	{
		public Address NewOrderAddress { get; }

		public OrderAddressChangedEvent(Guid aggregateId, long version, Address newOrderAddress)
			: base(aggregateId, version)
		{
			AggregateId = aggregateId;
			NewOrderAddress = newOrderAddress;
			Version = version;
		}
	}
}