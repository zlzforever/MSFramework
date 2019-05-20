using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
		public DefaultDbContext(DbContextOptions options, IMediator mediator,
			IEntityConfigurationTypeFinder typeFinder)
			: base(options, mediator, typeFinder, null)
		{
		}

		/// <summary>
		/// 初始化一个<see cref="DefaultDbContext"/>类型的新实例
		/// </summary>
		public DefaultDbContext(DbContextOptions options, IMediator mediator, IEntityConfigurationTypeFinder typeFinder,
			ILoggerFactory loggerFactory)
			: base(options, mediator, typeFinder, loggerFactory)
		{
		}
	}
}