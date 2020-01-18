using System;
using System.Collections.Generic;
using System.Linq;
using MSFramework.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Audit;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.Domain.Repository;
using MSFramework.Ef.Audit;

namespace MSFramework.Ef
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddEfAuditStore(this MSFrameworkBuilder builder)
		{
			builder.Services.AddScoped<IAuditStore, EfAuditStore>();
			return builder;
		}

		public static MSFrameworkBuilder AddEntityFramework(this MSFrameworkBuilder builder,
			Action<EntityFrameworkBuilder> configure, IConfiguration configuration)
		{
			configuration.NotNull(nameof(configuration));

			var eBuilder = new EntityFrameworkBuilder(builder.Services);
			configure?.Invoke(eBuilder);

			var section = configuration.GetSection("DbContexts");
			EntityFrameworkOptions.EntityFrameworkOptionDict =
				section.Get<Dictionary<string, EntityFrameworkOptions>>();
			if (EntityFrameworkOptions.EntityFrameworkOptionDict == null ||
			    EntityFrameworkOptions.EntityFrameworkOptionDict.Count == 0)
			{
				throw new MSFrameworkException("未能找到数据上下文配置");
			}

			var repeated = EntityFrameworkOptions.EntityFrameworkOptionDict.Values.GroupBy(m => m.DbContextType)
				.FirstOrDefault(m => m.Count() > 1);
			if (repeated != null)
			{
				throw new MSFrameworkException($"数据上下文配置中存在多个配置节点指向同一个上下文类型：{repeated.First().DbContextTypeName}");
			}

			if (Singleton<IEntityConfigurationTypeFinder>.Instance == null)
			{
				Singleton<IEntityConfigurationTypeFinder>.Instance = new EntityConfigurationTypeFinder();
			}

			Singleton<IEntityConfigurationTypeFinder>.Instance.Initialize();

			builder.Services.AddScoped<DbContextFactory>();
			builder.Services.AddScoped<IUnitOfWorkManager, UnitOfWorkManager>();
			builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
			builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
			builder.Services.AddScoped(typeof(EfRepository<>), typeof(EfRepository<>));
			builder.Services.AddScoped(typeof(EfRepository<,>), typeof(EfRepository<,>));
			return builder;
		}
	}
}