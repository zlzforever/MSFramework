using System.Threading.Tasks;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Shared;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.Infrastructure.Repositories
{
	public class OrderRepository
		: EfRepository<Order, ObjectId>, IOrderingRepository
	{
		public OrderRepository(DbContextFactory context) : base(context)
		{
		}

		public override Task<Order> FindAsync(ObjectId id)
		{
			return Store.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.Id == id);
		}
	}
}