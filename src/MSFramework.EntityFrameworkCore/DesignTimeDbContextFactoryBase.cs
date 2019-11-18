using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.EventBus;

namespace MSFramework.EntityFrameworkCore
{
	/// <summary>
	/// 设计时数据上下文实例工厂基类，用于执行数据迁移
	/// </summary>
	public abstract class DesignTimeDbContextFactoryBase<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
		where TDbContext : DbContext
	{
		/// <summary>
		/// 创建一个数据上下文实例
		/// </summary>
		/// <param name="args">参数</param>
		/// <returns></returns>
		public virtual TDbContext CreateDbContext(string[] args)
		{
			var services = new ServiceCollection();
			Configure(services);

			var entityConfigurationTypeFinder = new EntityConfigurationTypeFinder();
			entityConfigurationTypeFinder.Initialize();

			services.AddSingleton<IEntityConfigurationTypeFinder>(entityConfigurationTypeFinder);
			services.AddPassThroughEventBus();
			services.AddLogging();

			var section = GetConfiguration().GetSection("DbContexts");
			EntityFrameworkOptions.EntityFrameworkOptionDict =
				section.Get<Dictionary<string, EntityFrameworkOptions>>();
			if (EntityFrameworkOptions.EntityFrameworkOptionDict == null ||
			    EntityFrameworkOptions.EntityFrameworkOptionDict.Count == 0)
			{
				throw new MSFrameworkException("未能找到数据上下文配置");
			}

			var provider = services.BuildServiceProvider();
			var factory = new DbContextFactory(provider);
			return factory.GetDbContext(typeof(TDbContext)) as TDbContext;
		}

		protected abstract IConfiguration GetConfiguration();

		protected abstract void Configure(IServiceCollection services);
	}
}