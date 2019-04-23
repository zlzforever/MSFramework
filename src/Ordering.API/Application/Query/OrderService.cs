using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSFramework.Domain.Repository;
using Ordering.Domain.Aggregates;

namespace Ordering.API.Application.Query
{
	public class OrderService : IOrderService
	{
		private readonly IRepository<Order, Guid> _repository;

		public OrderService(IRepository<Order, Guid> repository)
		{
			_repository = repository;
		}

		public async System.Threading.Tasks.Task<List<Order>> GetAllOrdersAsync()
		{
			var orders = await _repository.GetAllListAsync();
			return orders;
		}

		public async System.Threading.Tasks.Task<Order> GetOrderAsync(Guid orderId)
		{
			var order = await _repository.GetAsync(orderId);
			return order;
		}
	}
	
}
