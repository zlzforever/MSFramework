using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;

#if !NETSTANDARD2_0

#endif

namespace MicroserviceFramework.Ef.MySql
{
    public static class ServiceCollectionExtensions
    {
        public static EntityFrameworkBuilder AddMySql<TDbContext>(
            this EntityFrameworkBuilder builder, IConfiguration configuration,
            Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = null) where TDbContext : DbContextBase
        {
            builder.Services.AddMySql<TDbContext>(configuration, mySqlOptionsAction);
            return builder;
        }

        public static EntityFrameworkBuilder AddMySql<TDbContext1, TDbContext2>(
            this EntityFrameworkBuilder builder, IConfiguration configuration,
            Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = null) where TDbContext1 : DbContextBase
            where TDbContext2 : DbContextBase
        {
            builder.Services.AddMySql<TDbContext1>(configuration, mySqlOptionsAction);
            builder.Services.AddMySql<TDbContext2>(configuration, mySqlOptionsAction);
            return builder;
        }

        public static EntityFrameworkBuilder AddMySql<TDbContext1, TDbContext2, TDbContext3>(
            this EntityFrameworkBuilder builder, IConfiguration configuration,
            Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = null) where TDbContext1 : DbContextBase
            where TDbContext2 : DbContextBase
            where TDbContext3 : DbContextBase
        {
            builder.Services.AddMySql<TDbContext1>(configuration, mySqlOptionsAction);
            builder.Services.AddMySql<TDbContext2>(configuration, mySqlOptionsAction);
            builder.Services.AddMySql<TDbContext3>(configuration, mySqlOptionsAction);

            return builder;
        }

        public static IServiceCollection AddMySql<TDbContext>(
            this IServiceCollection services, IConfiguration configuration,
            Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = null) where TDbContext : DbContextBase
        {
            var action = new Action<DbContextOptionsBuilder>(x =>
            {
                var dbContextType = typeof(TDbContext);
                var optionDict = configuration.GetSection("DbContexts").Get<DbContextConfigurationCollection>();
                var option = optionDict.Get(dbContextType);

                var entryAssemblyName = !string.IsNullOrWhiteSpace(option.MigrationsAssembly)
                    ? option.MigrationsAssembly
                    : dbContextType.Assembly.GetName().Name;

                x.UseMySql(option.ConnectionString, ServerVersion.AutoDetect(option.ConnectionString), options =>
                {
                    mySqlOptionsAction?.Invoke(options);

                    var migrationsHistoryTable = string.IsNullOrWhiteSpace(option.TablePrefix)
                        ? Defaults.MigrationsHistoryTable
                        : $"{option.TablePrefix}migrations_history";
                    
                    options.MigrationsHistoryTable(migrationsHistoryTable);

                    options.MaxBatchSize(option.MaxBatchSize);
                    options.MigrationsAssembly(entryAssemblyName);
                    options.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}_{table}");
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            });
            services.AddDbContext<TDbContext>(action);
            return services;
        }
    }
}
