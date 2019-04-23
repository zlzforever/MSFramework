using System;
using MSFramework.Command;
using Ordering.Domain.AggregateRoot;


namespace Ordering.API.Application.Command
{
	public class ChangeOrderAddressCommand : ICommand
    {
		public long Version { get; set; }

		public Address NewAddress { get; set; }
		
		public Guid OrderId { get; set; }
    }
}
