using MicroserviceFramework.Application;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options,
			IOptions<DbContextConfigurationCollection> entityFrameworkOptions,
			IDomainEventDispatcher domainEventDispatcher, ISession session, ILoggerFactory loggerFactory) : base(
			options, entityFrameworkOptions, domainEventDispatcher, session, loggerFactory)
		{
		}
	}
}