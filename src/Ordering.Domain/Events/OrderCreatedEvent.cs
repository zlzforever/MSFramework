using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSFramework.Domain;
using Ordering.Domain.Aggregates;

namespace Ordering.Domain.Events
{
	public class OrderCreatedEvent : AggregateEvent<Guid>
	{
		public string Description { get; private set; }

		public string Address { get; private set; }

		public List<OrderItem> OrderItems { get; private set; }

		public OrderCreatedEvent(
			Guid id,
			string description,
			string address,
			List<OrderItem> orderItems,
			long version
			)
			:base(id, version)
		{
			Description = description;
			Address = address;
			OrderItems = orderItems;
		}
	}
}
