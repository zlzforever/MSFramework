using System;
using MSFramework.Domain.Entity;

namespace Ordering.Domain.AggregateRoot.Order
{
	public class OrderItem : EntityBase<Guid>
	{
		public int ProductId { get; }

		public string PictureUrl { get; }

		public decimal Discount { get; private set; }

		public int Units { get; private set; }

		public decimal UnitPrice { get; }

		public string ProductName { get; }

		protected OrderItem()
		{
		}

		public OrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl,
			int units = 1)
		{
			if (units <= 0)
			{
				throw new OrderingException("Invalid number of units");
			}

			if ((unitPrice * units) < discount)
			{
				throw new OrderingException("The total of order item is lower than applied discount");
			}

			ProductId = productId;

			ProductName = productName;
			UnitPrice = unitPrice;
			Discount = discount;
			Units = units;
			PictureUrl = pictureUrl;
		}

		public void SetDiscount(decimal discount)
		{
			if (discount < 0)
			{
				throw new OrderingException("Discount is not valid");
			}

			Discount = discount;
		}

		public void AddUnits(int units)
		{
			if (units < 0)
			{
				throw new OrderingException("Invalid units");
			}

			Units += units;
		}
	}
}