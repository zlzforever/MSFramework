using MicroserviceFramework.Application;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options, IOptions<DbContextConfigurationCollection> entityFrameworkOptions, UnitOfWorkManager unitOfWorkManager, IDomainEventDispatcher domainEventDispatcher, ISession session) : base(options, entityFrameworkOptions, unitOfWorkManager, domainEventDispatcher, session)
		{
		}
	}
}