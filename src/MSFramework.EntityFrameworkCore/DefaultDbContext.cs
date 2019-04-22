using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.CQRS.EventSouring;
using MSFramework.Domain;
using MSFramework.EventBus;

namespace MSFramework.EntityFrameworkCore
{
	/// <summary>
	/// 默认EntityFramework数据上下文
	/// </summary>
	public class DefaultDbContext : DbContextBase
	{
		/// <summary>
		/// 初始化一个<see cref="DefaultDbContext"/>类型的新实例
		/// </summary>
		public DefaultDbContext(DbContextOptions options,
			IEntityConfigurationTypeFinder typeFinder,
			ILocalEventBus mediator, IDistributedEventBus distributedEventBus, IEventStore eventStore,
			EntityFrameworkOptions config)
			: base(options,
				typeFinder,
				mediator,
				distributedEventBus,
				eventStore, config, null)
		{
		}

		/// <summary>
		/// 初始化一个<see cref="DefaultDbContext"/>类型的新实例
		/// </summary>
		public DefaultDbContext(DbContextOptions options, IEntityConfigurationTypeFinder typeFinder,
			ILocalEventBus mediator, IDistributedEventBus distributedEventBus,
			IEventStore eventStore, EntityFrameworkOptions config,
			ILoggerFactory loggerFactory)
			: base(options, typeFinder, mediator, distributedEventBus, eventStore, config, loggerFactory)
		{
		}
	}
}