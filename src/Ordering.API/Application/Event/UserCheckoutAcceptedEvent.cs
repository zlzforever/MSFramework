using System.Collections.Generic;
using MSFramework.Domain;
using Ordering.API.Application.DTO;

namespace Ordering.API.Application.Event
{
	public class UserCheckoutAcceptedEvent : DistributedDomainEvent
	{
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