using System;
using Microsoft.EntityFrameworkCore;

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
		public DefaultDbContext(DbContextOptions options, IServiceProvider serviceProvider)
			: base(options, serviceProvider)
		{
		}
	}
}