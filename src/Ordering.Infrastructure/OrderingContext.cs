using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.Ef;
using MSFramework.EventBus;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options) : base(options)
		{
		}
	}
}