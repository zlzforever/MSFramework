using MediatR;
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
		public const string DefaultSchema = "ordering";

		public OrderingContext(DbContextOptions options, IEntityConfigurationTypeFinder typeFinder,
			IEventBus eventBus,
			IEventStore eventStore, EntityFrameworkOptions config,
			ILoggerFactory loggerFactory) : base(options, typeFinder, eventBus, eventStore, config,
			loggerFactory)
		{
		}
	}
}