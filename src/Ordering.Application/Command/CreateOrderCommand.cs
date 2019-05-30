using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Application.Command
{
	public class CreateOrderCommand: IRequest<IActionResult>
	{
		public string UserId { get; set; }

		public string City { get; set; }

		public string Street { get; set; }

		public string State { get; set; }

		public string Country { get; set; }

		public string ZipCode { get; set; }

		public string Description { get; set; }

		public List<OrderItemDTO> OrderItems { get; }

 
		public CreateOrderCommand(List<OrderItemDTO> basketItems, string userId, string city,
			string street, string state, string country, string zipcode, string description)
		{
			OrderItems = basketItems;
			UserId = userId;

			City = city;
			Street = street;
			State = state;
			Country = country;
			ZipCode = zipcode;
			Description = description;
		}
		
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
}