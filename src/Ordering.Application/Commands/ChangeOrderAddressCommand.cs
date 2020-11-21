using MicroserviceFramework.Application.CQRS;
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
