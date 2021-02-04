using MicroserviceFramework.Application;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Domain.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef
{
	/// <summary>
	/// 默认EntityFramework数据上下文
	/// </summary>
	public class DefaultDbContext : DbContextBase
	{
		public DefaultDbContext(DbContextOptions options,
			IOptions<DbContextConfigurationCollection> entityFrameworkOptions, UnitOfWorkManager unitOfWorkManager,
			IDomainEventDispatcher domainEventDispatcher, ISession session) : base(options, entityFrameworkOptions,
			unitOfWorkManager, domainEventDispatcher, session)
		{
		}
	}
}