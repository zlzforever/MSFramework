using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Domain;
using MSFramework.Domain.AggregateRoot;
using MSFramework.Ef.Infrastructure;

namespace MSFramework.Ef
{
	public class DbContextFactory : IDisposable
	{
		private readonly EntityFrameworkOptionsCollection _optionsCollection;
		private readonly IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;
		private readonly IServiceProvider _serviceProvider;

		private readonly ConcurrentDictionary<Type, DbContextBase> _dbContextDict =
			new ConcurrentDictionary<Type, DbContextBase>();

		public DbContextFactory( 
			EntityFrameworkOptionsCollection optionsCollection, IServiceProvider serviceProvider)
		{
			_entityConfigurationTypeFinder = serviceProvider.GetRequiredService<IEntityConfigurationTypeFinder>();
			_serviceProvider = serviceProvider;
			_optionsCollection = optionsCollection;
		}

		/// <summary>
		/// 通过数据上下文类型获取数据上下文配置
		/// </summary>
		/// <param name="dbContextType"></param>
		/// <returns></returns>
		public EntityFrameworkOptions GetDbContextOptions(Type dbContextType)
		{
			return _optionsCollection.Get(dbContextType);
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
			
			var unitOfWorkManager = _serviceProvider.GetRequiredService<IUnitOfWorkManager>();
			unitOfWorkManager.Register(context);

			if (resolveOptions.UseTransaction && context.Database.CurrentTransaction == null)
			{
				context.Database.BeginTransaction();
			}

			_dbContextDict.TryAdd(dbContextType, context);
			return context;
		}

		public void Dispose()
		{
			foreach (var kv in _dbContextDict)
			{
				kv.Value.Dispose();
			}

			_dbContextDict.Clear();
		}
	}
}