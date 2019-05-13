using System;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Application.DTO
{
	public class OrderItemDTO
	{
		public Guid ProductId { get; set; }

		public string ProductName { get; set; }

		public decimal UnitPrice { get; set; }

		public decimal Discount { get; set; }

		public int Units { get; set; }

		public string PictureUrl { get; set; }

		public OrderItem ToOrderItem()
		{
			return new OrderItem(ProductId, ProductName, UnitPrice, Discount, PictureUrl, Units);
		}
	}
}