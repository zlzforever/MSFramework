using MSFramework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ordering.Domain.Events
{
	public class OrderAddressChangedEvent : AggregateEvent<Guid>
	{
		public readonly string NewOrderAddress;

		public OrderAddressChangedEvent()
		{
		}

		public OrderAddressChangedEvent(Guid id, string newOrderAddress, long version)
			: base(id, version)
		{
			Id = id;
			NewOrderAddress = newOrderAddress;
			Version = version;
		}
	}
}
