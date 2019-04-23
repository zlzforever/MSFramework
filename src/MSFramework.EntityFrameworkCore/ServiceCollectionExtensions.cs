using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MSFramework.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Collections.Generic;
using MSFramework.Common;
using MSFramework.Domain.Repository;
using MSFramework.EntityFrameworkCore.Repository;
using MSFramework.EventSouring;
using MSFramework.Serialization;

namespace MSFramework.EntityFrameworkCore
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseEntityFramework(this MSFrameworkBuilder builder,
			IDbContextOptionsBuilderCreator dbContextOptionsBuilderCreator)
		{
			builder.Configuration.NotNull(nameof(builder.Configuration));

			var section = builder.Configuration.GetSection("DbContexts");
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
				Singleton<IEntityConfigurationTypeFinder>.Instance.Initialize();
				builder.Services.AddSingleton(Singleton<IEntityConfigurationTypeFinder>.Instance);
			}

			builder.Services.AddScoped<DbContextFactory>();

			builder.Services.AddSingleton<EntityFrameworkMigrateService>();
			builder.Services.AddSingleton(dbContextOptionsBuilderCreator);
			builder.Services.AddScoped(typeof(IEfRepository<>), typeof(EfRepository<,>));
			builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<,>));
			return builder;
		}

		public static MSFrameworkBuilder UseEntityFrameworkEventStore(this MSFrameworkBuilder builder)
		{
			builder.Services.AddSingleton<IEventStore, EfEventStore>();
			return builder;
		}
	}
}