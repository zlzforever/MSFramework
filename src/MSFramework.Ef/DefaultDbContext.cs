using EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MSFramework.Ef
{
	/// <summary>
	/// 默认EntityFramework数据上下文
	/// </summary>
	public class DefaultDbContext : DbContextBase
	{
		/// <summary>
		/// 初始化一个<see cref="DefaultDbContext"/>类型的新实例
		/// </summary>
		public DefaultDbContext(DbContextOptions options)
			: base(options)
		{
		}

		/// <summary>
		/// 初始化一个<see cref="DefaultDbContext"/>类型的新实例
		/// </summary>
		public DefaultDbContext(DbContextOptions options, IEventBus eventBus,
			ILoggerFactory loggerFactory)
			: base(options)
		{
		}
	}
}