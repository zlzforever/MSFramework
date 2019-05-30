using System;
using MediatR;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Application.Command
{
	public class ChangeOrderAddressCommand : IRequest<bool>
	{
		public Address NewAddress { get; set; }
		
		public Guid OrderId { get; set; }
    }
}
