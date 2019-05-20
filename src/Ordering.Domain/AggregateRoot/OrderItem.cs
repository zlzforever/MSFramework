using System;
using MSFramework.Domain;

namespace Ordering.Domain.AggregateRoot
{
	public class OrderItem : EntityBase<Guid>
	{
		 // DDD Patterns comment
        // Using private fields, allowed since EF Core 1.1, is a much better encapsulation
        // aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
        private string  _productName;
        private string  _pictureUrl;
        private decimal _unitPrice;
        private decimal _discount;
        private int     _units;
        private Guid _productId;

        protected OrderItem() { }

        public OrderItem(Guid productId, string productName, decimal unitPrice, decimal discount, string PictureUrl, int units = 1)
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
            _pictureUrl = PictureUrl;
        }
        
        public Guid GetProductId() => _productId;
        
        public string GetPictureUri() => _pictureUrl;

        public decimal GetCurrentDiscount()
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

        public string GetOrderItemProductName() => _productName;

        public void SetNewDiscount(decimal discount)
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