using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using Ordering.API.Application.Dto.Basket;
using Ordering.API.Application.Dto.Order;

namespace Ordering.API.Application.Command
{
	public class CreateOrderCommand
		: IRequest<bool>
	{
		public string UserId { get; private set; }

		public string UserName { get; private set; }

		public string City { get; private set; }

		public string Street { get; private set; }

		public string State { get; private set; }

		public string Country { get; private set; }

		public string ZipCode { get; private set; }

		public string CardNumber { get; private set; }

		public string CardHolderName { get; private set; }

		public DateTime CardExpiration { get; private set; }

		public string CardSecurityNumber { get; private set; }

		public int CardTypeId { get; private set; }

		public List<OrderItemDto> OrderItems { get; private set; }

		public CreateOrderCommand()
		{
			OrderItems = new List<OrderItemDto>();
		}

		public CreateOrderCommand(List<BasketItemDto> basketItems, string userId, string userName, string city,
			string street, string state, string country, string zipcode,
			string cardNumber, string cardHolderName, DateTime cardExpiration,
			string cardSecurityNumber, int cardTypeId) : this()
		{
			OrderItems = basketItems.Select(item =>
				new OrderItemDto
				{
					ProductId = int.TryParse(item.ProductId, out int id) ? id : -1,
					ProductName = item.ProductName,
					PictureUrl = item.PictureUrl,
					UnitPrice = item.UnitPrice,
					Units = item.Quantity
				}).ToList();
			UserId = userId;
			UserName = userName;
			City = city;
			Street = street;
			State = state;
			Country = country;
			ZipCode = zipcode;
			CardNumber = cardNumber;
			CardHolderName = cardHolderName;
			CardExpiration = cardExpiration;
			CardSecurityNumber = cardSecurityNumber;
			CardTypeId = cardTypeId;
			CardExpiration = cardExpiration;
		}
	}
}