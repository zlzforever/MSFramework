using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.EntityFrameworkCore;
using MSFramework.EventBus;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options, IEntityConfigurationTypeFinder typeFinder,
			IEventBus eventBus,
			ILoggerFactory loggerFactory) : base(options, typeFinder,
			loggerFactory)
		{
		}
	}
}