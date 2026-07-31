using System;
using System.Linq;
using System.Text.RegularExpressions;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Auditing;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Ef.Internal;
using MicroserviceFramework.Ef.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Ef;

/// <summary>
/// EF 服务集合扩展方法，提供 DbContext 注册与框架集成
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 <typeparamref name="TContext"/> 的 DbContext 连接池，
    /// 自动从 <c>DbContexts</c> 配置节点解析对应的 <see cref="DbContextSettings"/>，
    /// 并注入到回调中供 Provider 选择和配置。
    /// </summary>
    /// <typeparam name="TContext">DbContext 类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">应用配置</param>
    /// <param name="optionsAction">
    ///     接收已解析的 <see cref="DbContextSettings"/>、<see cref="IServiceProvider"/> 和
    ///     <see cref="DbContextOptionsBuilder"/>，完成 Provider 选择和额外配置。
    /// </param>
    public static IServiceCollection AddDbContextPool<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextSettings, IServiceProvider, DbContextOptionsBuilder> optionsAction)
        where TContext : DbContextBase
    {
        var settings = configuration.GetDbContextSettings<TContext>();

        return services.AddDbContextPool<TContext>((provider, builder) =>
        {
            optionsAction(settings, provider, builder);
        });
    }

    /// <param name="builder">框架构建器</param>
    extension(MicroserviceFrameworkBuilder builder)
    {
        /// <summary>
        /// 启用 EF 审计存储，将审计日志写入指定 DbContext
        /// </summary>
        /// <typeparam name="TDbContext">审计存储使用的 DbContext 类型</typeparam>
        /// <returns>框架构建器</returns>
        public MicroserviceFrameworkBuilder UseEfAuditing<TDbContext>()
            where TDbContext : DbContext
        {
            // EfUtilities.AuditingDbContextType = typeof(TDbContext);
            builder.Services.AddScoped<IAuditingStore, EfAuditingStore<TDbContext>>();
            return builder;
        }

        /// <summary>
        /// 启用 EntityFramework 核心扩展服务
        /// </summary>
        /// <returns>框架构建器</returns>
        public MicroserviceFrameworkBuilder UseEntityFramework()
        {
            builder.Services.AddEntityFrameworkExtension();
            return builder;
        }
    }

    /// <summary>
    /// 注册 EntityFramework 核心扩展服务（仓储、工作单元、配置查找器等）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddEntityFrameworkExtension(this IServiceCollection services)
    {
        services.TryAddSingleton<IEntityConfigurationTypeFinder, EntityConfigurationTypeFinder>();
        services.TryAddScoped<DbContextFactory>();
        services.TryAddScoped<IUnitOfWork, EfUnitOfWork>();
        services.TryAddScoped(typeof(IExternalEntityRepository<,>), typeof(ExternalEntityRepository<,>));
        var repoInterface = typeof(IRepository<,>);
        services.TryAddScoped(repoInterface, typeof(EfRepository<,>));
        // 无键仓储开放泛型注册：面向实现非泛型 IAggregateRoot 的复合主键聚合根
        var keylessRepoInterface = typeof(IRepository<>);
        services.TryAddScoped(keylessRepoInterface, typeof(EfRepository<>));

        // var repoMethodsCount = repoInterface.GetMethods().Length;

        var types = Utils.Runtime.GetAllTypes();
        foreach (var type in types)
        {
            if (type == null || type == typeof(IEfRepository))
            {
                continue;
            }

            var match = RepositoryRegex().Match(type.Name);

            if (type.IsInterface && Defaults.Types.Repository.IsAssignableFrom(type) && match.Success)
            {
                // 必须是默认的仓储接口， 如果有自定义接口， 需要自己实现仓储
                if (type.GetMethods().Length != 0)
                {
                    continue;
                }

                var repoInterfaceType = type.GetInterfaces().FirstOrDefault(x =>
                    x.IsInterface && x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRepository<,>));
                if (repoInterfaceType != null)
                {
                    // TODO:

                    // // 若有自定义实现， 则已经自动注入
                    // if (services.Any(x => x.ServiceType == type))
                    // {
                    //     continue;
                    // }

                    // var entityType = repoInterfaceType.GetGenericArguments()[0].FullName;
                    // var entityKeyType = repoInterfaceType.GetGenericArguments()[1].FullName;
                    // var name = ObjectId.GenerateNewId().ToString();
//                     var script = $$"""
//                                    public class R_{{name}}_Repo
//                                        : MicroserviceFramework.Ef.Repositories.EfRepository<{{entityType}},
//                                         {{entityKeyType}}>, {{type.FullName}}
//                                    {
//                                        public R_{{name}}_Repo(MicroserviceFramework.Ef.DbContextFactory context) : base(context)
//                                        {
//                                            UseQuerySplittingBehavior = true;
//                                        }
//                                    }
//                                    """;
                    // var repoType = DynamicCompileUtil.CreateType(script);
                    // services.AddScoped(type, repoType);
                }
            }
        }

        return services;
    }

    /// <summary>
    /// 设置 DbContext 的连接字符串
    /// </summary>
    /// <param name="builder">DbContext 选项构建器</param>
    /// <param name="connectionString">连接字符串</param>
    /// <typeparam name="T">选项扩展类型</typeparam>
    /// <exception cref="MicroserviceFrameworkException">未找到指定扩展时抛出</exception>
    public static void SetConnectionString<T>(this DbContextOptionsBuilder builder, string connectionString) where T :
        class,
        IDbContextOptionsExtension
    {
#pragma warning disable EF1001
        var extension = builder.Options.FindExtension<T>() as RelationalOptionsExtension;
        if (extension == null)
        {
            throw new MicroserviceFrameworkException($"FindExtension {typeof(T).Name} failed");
        }

        var b = extension.WithConnectionString(connectionString);
#pragma warning restore EF1001
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(b);
    }

    [GeneratedRegex("^I[A-Za-z0-9_]+(Repository)$")]
    private static partial Regex RepositoryRegex();
}
