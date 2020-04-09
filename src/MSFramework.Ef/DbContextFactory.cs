using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.Ef
{
	public class DbContextFactory : IDisposable
	{
		private readonly EntityFrameworkOptionsStore _optionsStore;
		private readonly IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;
		private readonly IServiceProvider _serviceProvider;

		private readonly ConcurrentDictionary<Type, DbContextBase> _dbContextDict =
			new ConcurrentDictionary<Type, DbContextBase>();

		public DbContextFactory(IEntityConfigurationTypeFinder entityConfigurationTypeFinder,
			EntityFrameworkOptionsStore optionsStore, IServiceProvider serviceProvider)
		{
			_entityConfigurationTypeFinder = entityConfigurationTypeFinder;
			_serviceProvider = serviceProvider;
			_optionsStore = optionsStore;
		}

		/// <summary>
		/// 通过数据上下文类型获取数据上下文配置
		/// </summary>
		/// <param name="dbContextType"></param>
		/// <returns></returns>
		private EntityFrameworkOptions GetDbContextOptions(Type dbContextType)
		{
			return _optionsStore.Get(dbContextType);
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
			var dbContextOptions = GetDbContextOptions(dbContextType);
			if (dbContextOptions == null)
			{
				throw new MSFrameworkException($"未找到数据上下文“{dbContextType}”对应的配置文件");
			}

			var dbContext = Create(dbContextOptions);
			return dbContext;
		}

		public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class, IAggregateRoot
		{
			return GetDbContext<TEntity>().Set<TEntity>();
		}

		public DbContextBase Create(EntityFrameworkOptions resolveOptions)
		{
			var dbContextType = resolveOptions.DbContextType;
			//已存在上下文对象，直接返回
			if (_dbContextDict.TryGetValue(dbContextType, out DbContextBase context))
			{
				return context;
			}

			//创建上下文实例
			context = (DbContextBase) _serviceProvider.GetRequiredService(dbContextType);

			if (resolveOptions.UseTransaction && context.Database.CurrentTransaction == null)
			{
				context.Database.BeginTransaction();
			}

			_dbContextDict.TryAdd(dbContextType, context);
			return context;
		}

		public DbContextBase[] GetAllDbContexts()
		{
			return _dbContextDict.Values.ToArray();
		}

		public void Dispose()
		{
			foreach (var kv in _dbContextDict)
			{
				kv.Value.Dispose();
			}
		}
	}
}