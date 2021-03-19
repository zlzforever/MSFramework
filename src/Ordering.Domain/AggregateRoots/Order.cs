using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Shared;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Domain.AggregateRoots
{
	[Description("订单表")]
	public class Order : AggregateRoot<ObjectId>, IOptimisticLock
	{
		// DDD Patterns comment
		// Using a private collection field, better for DDD Aggregate's encapsulation
		// so OrderItems cannot be added from "outside the AggregateRoot" directly to the collection,
		// but only through the method OrderAggrergateRoot.AddOrderItem() which includes behaviour.
		public virtual ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

		// DDD Patterns comment
		// Using private fields, allowed since EF Core 1.1, is a much better encapsulation
		// aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
		public DateTimeOffset CreationTime { get; private set; }

		/// <summary>
		/// Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
		/// </summary>
		[Description("地址")]
		public virtual Address Address { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		[Description("状态")]
		public OrderStatus OrderStatus { get; private set; }

		public string UserId { get; private set; }

		public string Description { get; private set; }

		private Order() : base(ObjectId.NewId())
		{
		}

		public Order(
			string userId,
			Address address,
			string description,
			List<OrderItem> orderItems
		) : this()
		{
			Address = address;
			UserId = userId;
			Description = description;
			OrderItems = orderItems;
			CreationTime = DateTimeOffset.Now;
			OrderStatus = OrderStatus.Submitted;

			// Add the OrderStarterDomainEvent to the domain events collection 
			// to be raised/dispatched when comitting changes into the Database [ After DbContext.SaveChanges() ]
			var orderStartedDomainEvent = new OrderStartedDomainEvent(this, userId);

			AddDomainEvent(orderStartedDomainEvent);
		}

		public void AddOrderItem(Guid productId, string productName, decimal unitPrice, decimal discount,
			string pictureUrl, int units = 1)
		{
			var existingOrderForProduct = OrderItems
				.SingleOrDefault(o => o.ProductId == productId);

			if (existingOrderForProduct != null)
			{
				//if previous line exist modify it with higher discount  and units..

				if (discount > existingOrderForProduct.Discount)
				{
					existingOrderForProduct.SetNewDiscount(discount);
				}

				existingOrderForProduct.AddUnits(units);
			}
			else
			{
				//add validated new order item

				var orderItem = new OrderItem(productId, productName, unitPrice, discount, pictureUrl, units);
				OrderItems.Add(orderItem);
			}
		}

		public void ChangeAddress(Address newAddress)
		{
			Address = newAddress ?? throw new ArgumentException(nameof(newAddress));
		}

		public void SetAwaitingValidationStatus()
		{
			if (OrderStatus == OrderStatus.Submitted)
			{
				AddDomainEvent(new OrderStatusChangedToAwaitingValidationDomainEvent(Id, OrderItems));
				OrderStatus = OrderStatus.AwaitingValidation;
			}
		}

		public void SetStockConfirmedStatus()
		{
			if (OrderStatus == OrderStatus.AwaitingValidation)
			{
				AddDomainEvent(new OrderStatusChangedToStockConfirmedDomainEvent(Id));

				OrderStatus = OrderStatus.StockConfirmed;
				Description = "All the items were confirmed with available stock.";
			}
		}

		public void SetPaidStatus()
		{
			if (OrderStatus == OrderStatus.StockConfirmed)
			{
				AddDomainEvent(new OrderStatusChangedToPaidDomainEvent(Id, OrderItems));

				OrderStatus = OrderStatus.Paid;
				Description =
					"The payment was performed at a simulated \"American Bank checking bank account ending on XX35071\"";
			}
		}

		public void AddEvent()
		{
			AddDomainEvent(new EmptyEvent());
		}

		public class EmptyEvent : DomainEvent
		{
		}

		public void SetShippedStatus()
		{
			if (OrderStatus != OrderStatus.Paid)
			{
				StatusChangeException(OrderStatus.Shipped);
			}

			OrderStatus = OrderStatus.Shipped;
			Description = "The order was shipped.";
			AddDomainEvent(new OrderShippedDomainEvent(this));
		}

		public void SetCancelledStatus()
		{
			if (OrderStatus == OrderStatus.Paid ||
			    OrderStatus == OrderStatus.Shipped)
			{
				StatusChangeException(OrderStatus.Cancelled);
			}

			OrderStatus = OrderStatus.Cancelled;
			Description = $"The order was cancelled.";
			AddDomainEvent(new OrderCancelledDomainEvent(this));
		}

		public void SetCancelledStatusWhenStockIsRejected(IEnumerable<Guid> orderStockRejectedItems)
		{
			if (OrderStatus == OrderStatus.AwaitingValidation)
			{
				OrderStatus = OrderStatus.Cancelled;

				var itemsStockRejectedProductNames = OrderItems
					.Where(c => orderStockRejectedItems.Contains(c.ProductId))
					.Select(c => c.ProductName);

				var itemsStockRejectedDescription = string.Join(", ", itemsStockRejectedProductNames);
				Description = $"The product items don't have stock: ({itemsStockRejectedDescription}).";
			}
		}

		private void StatusChangeException(OrderStatus orderStatusToChange)
		{
			throw new OrderingDomainException(
				$"Is not possible to change the order status from {OrderStatus} to {orderStatusToChange}.");
		}

		public string ConcurrencyStamp { get; set; }
	}
}