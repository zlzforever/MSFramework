using System;
using Ordering.Domain.AggregateRoot;

namespace Ordering.API.Application.DTO
{
	public class ChangeOrderAddressDTO
    {
		public Address NewAddress { get; set; }
		
		public Guid OrderId { get; set; }
    }
}
