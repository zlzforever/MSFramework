using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Ef;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public class OrderRepository
		: EfRepository<Order>, IOrderingRepository
	{
		public OrderRepository(DbContextFactory context) : base(context)
		{
		}

		public override Task<Order> GetAsync(Guid id)
		{
			return Entities.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.Id == id);
		}
	}
}