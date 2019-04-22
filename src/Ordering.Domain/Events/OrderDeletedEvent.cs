using MSFramework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ordering.Domain.Events
{
	public class OrderDeletedEvent : AggregateEvent<Guid>
	{
		public OrderDeletedEvent()
		{
		}

		public OrderDeletedEvent(Guid id, int version)
		{
			Id = id;
			Version = version;
		}
	}
}
