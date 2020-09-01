using MicroserviceFramework.Application;
using MicroserviceFramework.Application.CQRS.Command;
using MicroserviceFramework.Shared;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Commands
{
	public class ChangeOrderAddressCommand : ICommand
	{
		public Address NewAddress { get; set; }
		
		public ObjectId OrderId { get; set; }
    }
}
