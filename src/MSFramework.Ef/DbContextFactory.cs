using System;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Domain;
using MSFramework.Ef.Infrastructure;

namespace MSFramework.Ef
{
	public class DbContextFactory
	{
		private readonly IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;
		private readonly IServiceProvider _serviceProvider;

		public DbContextFactory(IServiceProvider serviceProvider)
		{
			_entityConfigurationTypeFinder = serviceProvider.GetRequiredService<IEntityConfigurationTypeFinder>();
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// 获取指定数据实体的上下文类型
		/// </summary>
		/// <returns>实体所属上下文实例</returns>
		public DbContextBase GetDbContext<TEntity>() where TEntity : class, IAggregateRoot
		{
			var dbContextType = _entityConfigurationTypeFinder.GetDbContextTypeForEntity(typeof(TEntity));
			return GetDbContext(dbContextType);
		}

		/// <summary>
		/// 通过数据上下文类型获取数据上下文对象
		/// </summary>
		/// <param name="dbContextType">数据上下文类型</param>
		/// <returns>数据上下文</returns>
		public DbContextBase GetDbContext(Type dbContextType)
		{
			var dbContext = (DbContextBase) _serviceProvider.GetRequiredService(dbContextType);
			return dbContext;
		}
	}
}