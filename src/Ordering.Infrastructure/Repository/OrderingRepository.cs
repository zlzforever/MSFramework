using System;
using System.Threading.Tasks;
using MSFramework.EntityFrameworkCore.Repository;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public class OrderingRepository : IOrderingRepository
	{
		private readonly EfRepository<Order, Guid> _efRepository;

		public OrderingRepository(EfRepository<Order, Guid> efRepository)
		{
			_efRepository = efRepository;
		}

		public Task<Order> GetAsync(Guid orderId)
		{
			return _efRepository.GetAsync(orderId);
		}

		public Task DeleteAsync(Order order)
		{
			return _efRepository.DeleteAsync(order);
		}

		public Task InsertAsync(Order order)
		{
			return _efRepository.InsertAsync(order);
		}
	}
}