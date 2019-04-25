using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Domain;
using MSFramework.EventSouring;

namespace MSFramework.EntityFrameworkCore
{
	public class DbContextFactory
	{
		private readonly IServiceProvider _serviceProvider;

		private readonly ConcurrentDictionary<Type, DbContextBase> _dbContextDict =
			new ConcurrentDictionary<Type, DbContextBase>();

		public DbContextFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// 通过数据上下文类型获取数据上下文配置
		/// </summary>
		/// <param name="dbContextType"></param>
		/// <returns></returns>
		private EntityFrameworkOptions GetDbContextOptions(Type dbContextType)
		{
			return EntityFrameworkOptions.EntityFrameworkOptionDict.Values.SingleOrDefault(m =>
				m.DbContextType == dbContextType);
		}

		/// <summary>
		/// 获取指定数据实体的上下文类型
		/// </summary>
		/// <returns>实体所属上下文实例</returns>
		public DbContext GetDbContext<TEntity>() where TEntity : class, IEntity
		{
			var typeFinder = _serviceProvider.GetService<IEntityConfigurationTypeFinder>();
			Type dbContextType = typeFinder.GetDbContextTypeForEntity(typeof(TEntity));
			return GetDbContext(dbContextType);
		}

		/// <summary>
		/// 通过数据上下文类型获取数据上下文对象
		/// </summary>
		/// <param name="dbContextType">数据上下文类型</param>
		/// <returns>数据上下文</returns>
		public DbContext GetDbContext(Type dbContextType)
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
			if (_dbContextDict.ContainsKey(dbContextType))
			{
				return _dbContextDict[dbContextType];
			}

			var builderCreator = _serviceProvider.GetServices<IDbContextOptionsBuilderCreator>()
				.FirstOrDefault(m => m.Type == resolveOptions.DatabaseType);
			if (builderCreator == null)
			{
				throw new MSFrameworkException(
					$"无法解析类型为“{resolveOptions.DatabaseType}”的 {typeof(IDbContextOptionsBuilderCreator).FullName} 实例");
			}

			DbContextOptionsBuilder optionsBuilder =
				builderCreator.Create(dbContextType, resolveOptions.ConnectionString);

			DbContextOptions options = optionsBuilder.Options;

			//创建上下文实例
			if (!(ActivatorUtilities.CreateInstance(_serviceProvider, dbContextType, options) is
				DbContextBase context))
			{
				throw new MSFrameworkException($"实例化数据上下文“{dbContextType.AssemblyQualifiedName}”失败");
			}

			_dbContextDict.TryAdd(dbContextType, context);
			return context;
		}

		public IEventStore GetEventStore()
		{
			return _serviceProvider.GetService<IEventStore>();
		}

		public DbContextBase[] GetAllDbContexts()
		{
			return _dbContextDict.Values.ToArray();
		}
	}
}