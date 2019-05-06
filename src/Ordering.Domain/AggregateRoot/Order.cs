using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace Ordering.Domain.AggregateRoot
{
	public class Order : AggregateRootBase<Order, Guid>
	{
		private string _description;
		private string _userId;

		// DDD Patterns comment
		// Using a private collection field, better for DDD Aggregate's encapsulation
		// so OrderItems cannot be added from "outside the AggregateRoot" directly to the collection,
		// but only through the method OrderAggrergateRoot.AddOrderItem() which includes behaviour.
		private List<OrderItem> _orderItems;

		// DDD Patterns comment
		// Using private fields, allowed since EF Core 1.1, is a much better encapsulation
		// aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
		private DateTime _creationTime;

		// Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
		public Address Address { get; private set; }

		public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

		protected Order()
		{
			_orderItems = new List<OrderItem>();
		}

		public Order(
			string userId,
			Address address,
			string description,
			List<OrderItem> orderItems
		)
		{
			ApplyChangedEvent(new OrderCreatedEvent(userId,
				address,
				description,
				orderItems));
		}

		private void Apply(OrderCreatedEvent @event)
		{
			Address = @event.Address;
			_userId = @event.UserId;
			_description = @event.Description;
			_orderItems = @event.OrderItems;
			_creationTime = @event.Timestamp;
		}

		private void Apply(OrderAddressChangedEvent @event)
		{
			Address = @event.NewOrderAddress;
		}

		private void Apply(OrderDeletedEvent @event)
		{
		}

		public void ChangeAddress(Address newAddress)
		{
			if (newAddress == null)
			{
				throw new ArgumentException(nameof(newAddress));
			}

			ApplyChangedEvent(new OrderAddressChangedEvent(newAddress));
		}

		public void Delete()
		{
			ApplyChangedEvent(new OrderDeletedEvent());
		}
	}
}