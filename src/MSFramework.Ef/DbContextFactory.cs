using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef
{
	public class DbContextFactory
	{
		private readonly IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;
		private readonly IServiceProvider _serviceProvider;

		public DbContextFactory(IServiceProvider serviceProvider)
		{
			_entityConfigurationTypeFinder = serviceProvider
				.GetRequiredService<IEntityConfigurationTypeFinder>();
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// 获取指定数据实体的上下文类型
		/// </summary>
		/// <returns>实体所属上下文实例</returns>
		public DbContextBase GetDbContext<TEntity>() where TEntity : class, IAggregateRoot
		{
			var dbContextType = _entityConfigurationTypeFinder
				.GetDbContextTypeForEntity(typeof(TEntity));
			return (DbContextBase) _serviceProvider.GetRequiredService(dbContextType);
		}

		public DbContextBase GetDbContextOrDefault<TEntity>() where TEntity : class, IAggregateRoot
		{
			if (!_entityConfigurationTypeFinder.HasDbContextForEntity<TEntity>())
			{
				return null;
			}

			return GetDbContext<TEntity>();
		}
	}
}