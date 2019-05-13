using System.Collections.Generic;

namespace Ordering.Application.DTO
{
	public class CreateOrderDTO
	{
		public string UserId { get; set; }

		public string City { get; set; }

		public string Street { get; set; }

		public string State { get; set; }

		public string Country { get; set; }

		public string ZipCode { get; set; }

		public string Description { get; set; }

		public List<OrderItemDTO> OrderItems { get; }

		public CreateOrderDTO()
		{
		}

		public CreateOrderDTO(List<OrderItemDTO> basketItems, string userId, string city,
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
	}
}