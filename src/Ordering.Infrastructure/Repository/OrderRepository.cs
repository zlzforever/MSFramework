using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Common;
using MSFramework.Ef;
using MSFramework.Ef.Repository;
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

		public override Task<Order> GetAsync(ObjectId id)
		{
			return CurrentSet.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.Id == id);
		}
	}
}