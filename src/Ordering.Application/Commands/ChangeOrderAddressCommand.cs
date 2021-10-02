using MicroserviceFramework.Application.CQRS;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Commands
{
	public class ChangeOrderAddressCommand : ICommand
	{
		public Address NewAddress { get; set; }
		
		public ObjectId OrderId { get; set; }
    }
}
