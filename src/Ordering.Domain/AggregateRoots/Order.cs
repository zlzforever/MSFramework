using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Domain.AggregateRoots
{
	[Description("订单表")]
	public class Order : CreationAggregateRoot, IOptimisticLock
	{
		// DDD Patterns comment
		// Using a private collection field, better for DDD Aggregate's encapsulation
		// so Items cannot be added from "outside the AggregateRoot" directly to the collection,
		// but only through the method OrderAggrergateRoot.AddOrderItem() which includes behaviour.
		private List<OrderItem> _items;

		public virtual IReadOnlyCollection<OrderItem> Items => _items;

		/// <summary>
		/// Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
		/// </summary>
		[Description("地址")]
		public virtual Address Address { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		[Description("状态")]
		public OrderStatus Status { get; private set; }

		public string BuyerId { get; private set; }

		public string Description { get; private set; }

		private Order(ObjectId id) : base(id)
		{
			_items = new List<OrderItem>();
		}

		// private Order(ILazyLoader lazyLoader, ObjectId id) : this(id)
		// {
		// 	_lazyLoader = lazyLoader;
		// 	_lazyLoader.Load(this, ref Items);
		// }

		private Order(
			string userId,
			Address address,
			string description
		) : this(ObjectId.GenerateNewId())
		{
			Address = address;
			BuyerId = userId;
			Description = description;


			Status = OrderStatus.Submitted;

			// Add the OrderStarterDomainEvent to the domain events collection 
			// to be raised/dispatched when comitting changes into the Database [ After DbContext.SaveChanges() ]
			var orderStartedDomainEvent = new OrderStartedDomainEvent(this, userId);

			AddDomainEvent(orderStartedDomainEvent);
		}

		public static Order Create(string buyerId,
			Address address,
			string description)
		{
			return new Order(buyerId,
					address,
					description)
				;
		}

		public void AddItem(Guid productId, string productName, decimal unitPrice, decimal discount,
			string pictureUrl, int units = 1)
		{
			var existingOrderForProduct = Items
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
				_items.Add(orderItem);
			}
		}

		public void ChangeAddress(Address newAddress)
		{
			Address = newAddress ?? throw new ArgumentException(nameof(newAddress));
		}

		public void SetAwaitingValidationStatus()
		{
			if (Status == OrderStatus.Submitted)
			{
				AddDomainEvent(new OrderStatusChangedToAwaitingValidationDomainEvent(Id, Items));
				Status = OrderStatus.AwaitingValidation;
			}
		}

		public void SetStockConfirmedStatus()
		{
			if (Status == OrderStatus.AwaitingValidation)
			{
				AddDomainEvent(new OrderStatusChangedToStockConfirmedDomainEvent(Id));

				Status = OrderStatus.StockConfirmed;
				Description = "All the items were confirmed with available stock.";
			}
		}

		public void SetPaidStatus()
		{
			if (Status == OrderStatus.StockConfirmed)
			{
				AddDomainEvent(new OrderStatusChangedToPaidDomainEvent(Id, Items));

				Status = OrderStatus.Paid;
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
			if (Status != OrderStatus.Paid)
			{
				StatusChangeException(OrderStatus.Shipped);
			}

			Status = OrderStatus.Shipped;
			Description = "The order was shipped.";
			AddDomainEvent(new OrderShippedDomainEvent(this));
		}

		public void SetCancelledStatus()
		{
			if (Status == OrderStatus.Paid ||
			    Status == OrderStatus.Shipped)
			{
				StatusChangeException(OrderStatus.Cancelled);
			}

			Status = OrderStatus.Cancelled;
			Description = $"The order was cancelled.";
			AddDomainEvent(new OrderCancelledDomainEvent(this));
		}

		public void SetCancelledStatusWhenStockIsRejected(IEnumerable<Guid> orderStockRejectedItems)
		{
			if (Status == OrderStatus.AwaitingValidation)
			{
				Status = OrderStatus.Cancelled;

				var itemsStockRejectedProductNames = Items
					.Where(c => orderStockRejectedItems.Contains(c.ProductId))
					.Select(c => c.ProductName);

				var itemsStockRejectedDescription = string.Join(", ", itemsStockRejectedProductNames);
				Description = $"The product items don't have stock: ({itemsStockRejectedDescription}).";
			}
		}

		private void StatusChangeException(OrderStatus orderStatusToChange)
		{
			throw new OrderingDomainException(
				$"Is not possible to change the order status from {Status} to {orderStatusToChange}.");
		}

		public string ConcurrencyStamp { get; set; }
	}
}