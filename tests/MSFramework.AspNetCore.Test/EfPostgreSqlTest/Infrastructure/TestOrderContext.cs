using MicroserviceFramework.Application;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure
{
	public class TestDataContext : DbContextBase
	{
		public TestDataContext(DbContextOptions options,
			IOptions<DbContextConfigurationCollection> entityFrameworkOptions, UnitOfWorkManager unitOfWorkManager,
			IDomainEventDispatcher domainEventDispatcher, ISession session) : base(options, entityFrameworkOptions,
			unitOfWorkManager, domainEventDispatcher, session)
		{
		}
	}
}