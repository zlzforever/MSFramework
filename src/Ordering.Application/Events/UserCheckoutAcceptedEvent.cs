using System;
using System.Collections.Generic;
using MicroserviceFramework.Domain.Events;

namespace Ordering.Application.Events
{
	public class UserCheckoutAcceptedEvent : Event
	{
		public class OrderItemDTO
		{
			public Guid ProductId { get; set; }

			public string ProductName { get; set; }

			public decimal UnitPrice { get; set; }

			public decimal Discount { get; set; }

			public int Units { get; set; }

			public string PictureUrl { get; set; }
		}

		public string UserId { get; }

		public string City { get; set; }

		public string Street { get; set; }

		public string State { get; set; }

		public string Country { get; set; }

		public string ZipCode { get; set; }

		public string Description { get; }

		public List<OrderItemDTO> OrderItems { get; set; }

		public UserCheckoutAcceptedEvent(List<OrderItemDTO> basketItems, string userId, string city, string street,
			string state, string country, string zipCode, string description)
		{
			OrderItems = basketItems;
			UserId = userId;
			City = city;
			Street = street;
			State = state;
			Country = country;
			ZipCode = zipCode;
			Description = description;
		}
	}
}