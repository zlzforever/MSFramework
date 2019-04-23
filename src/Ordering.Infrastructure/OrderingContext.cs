using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using MSFramework.EntityFrameworkCore;
using MSFramework.EventBus;
using MSFramework.EventSouring;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options, IEntityConfigurationTypeFinder typeFinder,
			IEventBus eventBus,
			ILoggerFactory loggerFactory) : base(options, typeFinder, eventBus,
			loggerFactory)
		{
		}
	}
}