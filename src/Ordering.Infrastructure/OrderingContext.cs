using Microsoft.EntityFrameworkCore;
using MSFramework.Ef;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options) : base(options)
		{
		}
	}
}