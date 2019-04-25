using System;
using MSFramework.Domain;

namespace Ordering.Domain.AggregateRoot
{
	public class OrderItem : EntityBase<Guid>
	{
		// DDD Patterns comment
		// Using private fields, allowed since EF Core 1.1, is a much better encapsulation
		// aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
		private decimal _unitPrice;
		private decimal _discount;
		private int _units;
		private Guid _productId;
		private string _pictureUrl;
		private string _productName;

		public Guid GetProductId()
		{
			return _productId;
		}

		public string GetPictureUrl()
		{
			return _pictureUrl;
		}

		public decimal GetDiscount()
		{
			return _discount;
		}

		public int GetUnits()
		{
			return _units;
		}

		public decimal GetUnitPrice()
		{
			return _unitPrice;
		}

		public string GetProductName()
		{
			return _productName;
		}

		private OrderItem()
		{
		}

		public OrderItem(Guid productId, string productName, decimal unitPrice, decimal discount, string pictureUrl,
			int units = 1)
		{
			if (units <= 0)
			{
				throw new OrderingDomainException("Invalid number of units");
			}

			if ((unitPrice * units) < discount)
			{
				throw new OrderingDomainException("The total of order item is lower than applied discount");
			}

			_productId = productId;

			_productName = productName;
			_unitPrice = unitPrice;
			_discount = discount;
			_units = units;
			_pictureUrl = pictureUrl;
		}

		public void SetUnitPrice(decimal price)
		{
			_unitPrice = price;
		}

		public void SetDiscount(decimal discount)
		{
			if (discount < 0)
			{
				throw new OrderingDomainException("Discount is not valid");
			}

			_discount = discount;
		}

		public void AddUnits(int units)
		{
			if (units < 0)
			{
				throw new OrderingDomainException("Invalid units");
			}

			_units += units;
		}
	}
}