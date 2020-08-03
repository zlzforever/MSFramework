using System;
using MSFramework.Common;
using MSFramework.Domain.Entity;

namespace Ordering.Domain.AggregateRoot
{
	public class OrderItem : EntityBase<ObjectId>
	{
		// DDD Patterns comment
		// Using private fields, allowed since EF Core 1.1, is a much better encapsulation
		// aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
		public string ProductName { get; private set; }
		public string PictureUrl { get; private set; }
		public decimal UnitPrice { get; private set; }
		public decimal Discount { get; private set; }
		public int Units { get; private set; }
		public Guid ProductId { get; private set; }

		private OrderItem() : base(ObjectId.NewId())
		{
		}

		public OrderItem(Guid productId, string productName, decimal unitPrice, decimal discount, string pictureUrl,
			int units = 1) : this()
		{
			if (units <= 0)
			{
				throw new OrderingDomainException("Invalid number of units");
			}

			if ((unitPrice * units) < discount)
			{
				throw new OrderingDomainException("The total of order item is lower than applied discount");
			}

			ProductId = productId;
			ProductName = productName;
			UnitPrice = unitPrice;
			Discount = discount;
			Units = units;
			PictureUrl = pictureUrl;
		}


		public void SetNewDiscount(decimal discount)
		{
			if (discount < 0)
			{
				throw new OrderingDomainException("Discount is not valid");
			}

			Discount = discount;
		}

		public void AddUnits(int units)
		{
			if (units < 0)
			{
				throw new OrderingDomainException("Invalid units");
			}

			Units += units;
		}
	}
}