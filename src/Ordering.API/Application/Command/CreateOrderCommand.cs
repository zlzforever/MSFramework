using System.Collections.Generic;
using MSFramework.Command;
using Ordering.API.Application.DTO;


namespace Ordering.API.Application.Command
{
	public class CreateOrderCommand : ICommand
	{
		public string UserId { get; }

		public string City { get; }

		public string Street { get; }

		public string State { get; }

		public string Country { get; }

		public string ZipCode { get; }

		public string Description { get; }

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
	}
}