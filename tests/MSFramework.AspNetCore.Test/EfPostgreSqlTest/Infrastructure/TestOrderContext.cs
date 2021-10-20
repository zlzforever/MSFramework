using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure
{
	public class TestDataContext : DbContextBase
	{
		public TestDataContext(DbContextOptions options,
			IOptions<DbContextConfigurationCollection> entityFrameworkOptions,
			IMediator domainEventDispatcher, ISession session, ILoggerFactory loggerFactory) : base(
			options, entityFrameworkOptions, domainEventDispatcher, session, loggerFactory)
		{
		}
	}
}