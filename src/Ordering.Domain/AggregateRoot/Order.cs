using System;
using System.Collections.Generic;
using System.Linq;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot.Event;

namespace Ordering.Domain.AggregateRoot
{
	public class Order : AggregateRootBase
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
		private DateTimeOffset _creationTime;

		// Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
		public Address Address { get; private set; }

		public OrderStatus OrderStatus { get; private set; }
		
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
			Address = address;
			_userId = userId;
			_description = description;
			_orderItems = orderItems;
			_creationTime = DateTimeOffset.Now;

			// Add the OrderStarterDomainEvent to the domain events collection 
			// to be raised/dispatched when comitting changes into the Database [ After DbContext.SaveChanges() ]
			var orderStartedDomainEvent = new OrderStartedDomainEvent(this, userId);

			AddDomainEvent(orderStartedDomainEvent);
		}

		public void AddOrderItem(Guid productId, string productName, decimal unitPrice, decimal discount,
			string pictureUrl, int units = 1)
		{
			var existingOrderForProduct = _orderItems
				.SingleOrDefault(o => o.GetProductId() == productId);

			if (existingOrderForProduct != null)
			{
				//if previous line exist modify it with higher discount  and units..

				if (discount > existingOrderForProduct.GetCurrentDiscount())
				{
					existingOrderForProduct.SetNewDiscount(discount);
				}

				existingOrderForProduct.AddUnits(units);
			}
			else
			{
				//add validated new order item

				var orderItem = new OrderItem(productId, productName, unitPrice, discount, pictureUrl, units);
				_orderItems.Add(orderItem);
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
                AddDomainEvent(new OrderStatusChangedToAwaitingValidationDomainEvent(Id, _orderItems));
                OrderStatus = OrderStatus.AwaitingValidation;
            }
        }

        public void SetStockConfirmedStatus()
        {
            if (OrderStatus == OrderStatus.AwaitingValidation)
            {
                AddDomainEvent(new OrderStatusChangedToStockConfirmedDomainEvent(Id));

                OrderStatus = OrderStatus.StockConfirmed;
                _description = "All the items were confirmed with available stock.";
            }
        }

        public void SetPaidStatus()
        {
            if (OrderStatus == OrderStatus.StockConfirmed)
            {
                AddDomainEvent(new OrderStatusChangedToPaidDomainEvent(Id, OrderItems));

                OrderStatus = OrderStatus.Paid;
                _description = "The payment was performed at a simulated \"American Bank checking bank account ending on XX35071\"";
            }
        }

        public void SetShippedStatus()
        {
            if (OrderStatus != OrderStatus.Paid)
            {
                StatusChangeException(OrderStatus.Shipped);
            }

            OrderStatus = OrderStatus.Shipped;
            _description = "The order was shipped.";
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
            _description = $"The order was cancelled.";
            AddDomainEvent(new OrderCancelledDomainEvent(this));
        }

        public void SetCancelledStatusWhenStockIsRejected(IEnumerable<Guid> orderStockRejectedItems)
        {
            if (OrderStatus == OrderStatus.AwaitingValidation)
            {
	            OrderStatus = OrderStatus.Cancelled;

                var itemsStockRejectedProductNames = OrderItems
                    .Where(c => orderStockRejectedItems.Contains(c.GetProductId()))
                    .Select(c => c.GetOrderItemProductName());

                var itemsStockRejectedDescription = string.Join(", ", itemsStockRejectedProductNames);
                _description = $"The product items don't have stock: ({itemsStockRejectedDescription}).";
            }
        }
        
        private void StatusChangeException(OrderStatus orderStatusToChange)
        {
	        throw new OrderingDomainException($"Is not possible to change the order status from {OrderStatus} to {orderStatusToChange}.");
        }
	}
}