using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
			IEntityConfigurationTypeFinder typeFinder)
			: base(options, typeFinder, null)
		{
		}

		/// <summary>
		/// 初始化一个<see cref="DefaultDbContext"/>类型的新实例
		/// </summary>
		public DefaultDbContext(DbContextOptions options, IEntityConfigurationTypeFinder typeFinder,
			ILoggerFactory loggerFactory)
			: base(options, typeFinder, loggerFactory)
		{
		}
	}
}