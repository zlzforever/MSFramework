using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.EntityFrameworkCore;
using MSFramework.EventBus;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options, IEventBus eventBus, IEntityConfigurationTypeFinder typeFinder,
			ILoggerFactory loggerFactory) : base(options, eventBus, typeFinder,
			loggerFactory)
		{
		}
	}
}