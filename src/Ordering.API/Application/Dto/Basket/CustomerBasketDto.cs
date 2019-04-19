using System.Collections.Generic;

namespace Ordering.API.Application.Dto.Basket
{
	public class CustomerBasketDto
	{
		public string BuyerId { get; set; }
		public List<BasketItemDto> Items { get; set; }

		public CustomerBasketDto(string customerId)
		{
			BuyerId = customerId;
			Items = new List<BasketItemDto>();
		}
	}
}