using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace Ordering.Domain.AggregateRoot
{
	public class Order : AggregateRootBase<Guid>
	{
		private string _description;
		private bool _isDeleted;
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

		public int OrderStatus { get; private set; }

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
			ApplyAggregateEvent(new OrderCreatedEvent(userId,
				address,
				description,
				orderItems));
		}

		private void Apply(OrderCreatedEvent e)
		{
			Version = e.Version;

			Id = e.AggregateId;
			Address = e.Address;
			_userId = e.UserId;
			_description = e.Description;
			_orderItems = e.OrderItems;
			_creationTime = e.CreationTime;
			OrderStatus= 0;
		}

		private void Apply(OrderAddressChangedEvent e)
		{
			Version = e.Version;
			Address = e.NewOrderAddress;
		}

		private void Apply(OrderDeletedEvent e)
		{
			Version = e.Version;
			_isDeleted = true;
		}

		public void ChangeAddress(Address newAddress)
		{
			if (newAddress == null)
			{
				throw new ArgumentException(nameof(newAddress));
			}

			ApplyAggregateEvent(new OrderAddressChangedEvent(Id, Version, newAddress));
		}

		public void Delete()
		{
			ApplyAggregateEvent(new OrderDeletedEvent(Id, Version));
		}
	}
}