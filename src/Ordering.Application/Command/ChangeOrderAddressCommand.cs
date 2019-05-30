using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Application.Command
{
	public class ChangeOrderAddressCommand : IRequest<IActionResult>
	{
		public Address NewAddress { get; set; }
		
		public Guid OrderId { get; set; }
    }
}
