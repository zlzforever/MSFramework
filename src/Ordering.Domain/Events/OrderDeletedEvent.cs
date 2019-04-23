using MSFramework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ordering.Domain.Events
{
	public class OrderDeletedEvent : AggregateEvent<Guid>
	{
		public OrderDeletedEvent(Guid id, long version)
			:base(id, version)
		{

		}
	}
}
