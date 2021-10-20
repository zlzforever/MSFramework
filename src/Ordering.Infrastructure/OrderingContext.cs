using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options,
			IOptions<DbContextConfigurationCollection> entityFrameworkOptions,
			IMediator domainEventDispatcher, ISession session, ILoggerFactory loggerFactory) : base(
			options, entityFrameworkOptions, domainEventDispatcher, session, loggerFactory)
		{
		}
	}
}