using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public class OrderRepository : IOrderRepository
	{
		private readonly EfRepository<Order, Guid> _efRepository;

		public OrderRepository(EfRepository<Order, Guid> efRepository)
		{
			_efRepository = efRepository;
		}

		public Task<Order> GetAsync(Guid orderId)
		{
			return _efRepository.GetAsync(orderId);
		}

		public Task InsertAsync(Order order)
		{
			return _efRepository.InsertAsync(order);
		}

		public Task<List<Order>> GetAllListAsync()
		{
			return _efRepository.AggregateRoots.AsNoTracking().ToListAsync();
		}
	}
}