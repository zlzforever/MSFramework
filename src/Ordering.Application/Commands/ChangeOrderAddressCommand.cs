using MSFramework.Application;
using MSFramework.Common;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Commands
{
	public class ChangeOrderAddressCommand : IRequest<int>
	{
		public Address NewAddress { get; set; }
		
		public ObjectId OrderId { get; set; }
    }
}
