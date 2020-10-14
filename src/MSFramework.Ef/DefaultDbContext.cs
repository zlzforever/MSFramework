using System;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef
{
	/// <summary>
	/// 默认EntityFramework数据上下文
	/// </summary>
	public class DefaultDbContext : DbContextBase
	{
		public DefaultDbContext(DbContextOptions options,
			UnitOfWorkManager unitOfWorkManager,
			IServiceProvider serviceProvider) : base(options, unitOfWorkManager, serviceProvider)
		{
		}
	}
}