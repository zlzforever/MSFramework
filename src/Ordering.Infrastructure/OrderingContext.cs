using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.CQRS.EventSouring;
using MSFramework.Domain;
using MSFramework.EntityFrameworkCore;
using MSFramework.EventBus;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public const string DefaultSchema = "ordering";

		public OrderingContext(DbContextOptions options, IEntityConfigurationTypeFinder typeFinder,
			ILocalEventBus mediator, IDistributedEventBus distributedEventBus,
			IEventStore eventStore, EntityFrameworkOptions config,
			ILoggerFactory loggerFactory) : base(options, typeFinder, mediator, distributedEventBus, eventStore, config,
			loggerFactory)
		{
		}
	}
}