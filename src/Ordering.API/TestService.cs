using System;
using System.Collections.Generic;
using MSFramework.Domain.Repository;
using Ordering.Domain.AggregateRoot;

namespace Ordering.API
{
	public class TestService
	{
		private IRepository<Order> _repository;

		public TestService(IRepository<Order> repository)
		{
			_repository = repository;
		}

		public string Get(string name)
		{
			var address = new Address("a", "b", "c", "d", "e");
			var order = new Order("test",
				address, "asdfa", new List<OrderItem>
				{
					new OrderItem(Guid.NewGuid(), "p1", 10, 0, "", 2)
				}
			);

			_repository.Insert(order);
			return name;
		}
	}
}