using MicroserviceFramework.Mediator;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Commands
{
    public class ChangeOrderAddressCommand : IRequest
    {
        public Address NewAddress { get; set; }

        public ObjectId OrderId { get; set; }
    }
}