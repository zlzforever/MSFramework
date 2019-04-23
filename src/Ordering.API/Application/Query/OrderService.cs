using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSFramework.DependencyInjection;
using MSFramework.Domain.Repository;
using Ordering.Domain.AggregateRoot;

namespace Ordering.API.Application.Query
{
	public class OrderService : IOrderService, IScopeDependency
	{
		private readonly IRepository<Order, Guid> _repository;

		public OrderService(IRepository<Order, Guid> repository)
		{
			_repository = repository;
		}

		public async Task<List<Order>> GetAllOrdersAsync()
		{
			var orders = await _repository.GetAllListAsync();
			return orders;
		}

		public async Task<Order> GetOrderAsync(Guid orderId)
		{
			var order = await _repository.GetAsync(orderId);
			return order;
		}
	}
}