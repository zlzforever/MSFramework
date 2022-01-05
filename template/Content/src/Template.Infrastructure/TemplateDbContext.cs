using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Template.Infrastructure
{
	public class TemplateDbContext : DbContextBase
	{
		public TemplateDbContext(DbContextOptions options,
			IOptions<DbContextConfigurationCollection> dbContextConfigurationCollection,
			IMediator domainEventDispatcher, ISession session, ILoggerFactory loggerFactory) : base(
			options, dbContextConfigurationCollection, domainEventDispatcher, session, loggerFactory)
		{
		}
	}
}