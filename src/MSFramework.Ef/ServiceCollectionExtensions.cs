using System.Linq;
using System.Text.RegularExpressions;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Auditing;
using MicroserviceFramework.Ef.Internal;
using MicroserviceFramework.Ef.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;

namespace MicroserviceFramework.Ef;

/// <summary>
///
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddEfAuditing<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        EfUtilities.AuditingDbContextType = typeof(TDbContext);
        services.AddScoped<IAuditingStore, EfAuditingStore<TDbContext>>();
        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseEfAuditing<TDbContext>(this MicroserviceFrameworkBuilder builder)
        where TDbContext : DbContext
    {
        EfUtilities.AuditingDbContextType = typeof(TDbContext);
        builder.Services.AddScoped<IAuditingStore, EfAuditingStore<TDbContext>>();
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseEntityFramework(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddEntityFrameworkExtension();
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEntityFrameworkExtension(this IServiceCollection services)
    {
        services.TryAddSingleton<IEntityConfigurationTypeFinder, EntityConfigurationTypeFinder>();
        services.TryAddScoped<DbContextFactory>();
        services.TryAddScoped<IUnitOfWork, EfUnitOfWork>();
        services.TryAddScoped(typeof(IExternalEntityRepository<,>), typeof(ExternalEntityRepository<,>));
        var repoInterface = typeof(IRepository<,>);
        services.TryAddScoped(repoInterface, typeof(EfRepository<,>));

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
                    x.IsInterface && x.GetGenericTypeDefinition() == typeof(IRepository<,>));
                if (repoInterfaceType != null)
                {
                    // 若有自定义实现， 则已经自动注入
                    if (services.Any(x => x.ServiceType == type))
                    {
                        continue;
                    }

                    var entityType = repoInterfaceType.GetGenericArguments()[0].FullName;
                    var entityKeyType = repoInterfaceType.GetGenericArguments()[1].FullName;
                    var name = ObjectId.GenerateNewId().ToString();
                    var script = $$"""
                                   public class R_{{name}}_Repo
                                       : MicroserviceFramework.Ef.Repositories.EfRepository<{{entityType}},
                                        {{entityKeyType}}>, {{type.FullName}}
                                   {
                                       public R_{{name}}_Repo(MicroserviceFramework.Ef.DbContextFactory context) : base(context)
                                       {
                                           UseQuerySplittingBehavior = true;
                                       }
                                   }
                                   """;
                    // var repoType = DynamicCompileUtil.CreateType(script);
                    // services.AddScoped(type, repoType);
                }
            }
        }

        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="connectionString"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="MicroserviceFrameworkException"></exception>
    public static void SetConnectionString<T>(this DbContextOptionsBuilder builder, string connectionString) where T :
        class,
        IDbContextOptionsExtension
    {
#pragma warning disable EF1001
        var extension = builder.Options.FindExtension<T>() as RelationalOptionsExtension;
        if (extension == null)
        {
            throw new MicroserviceFrameworkException("NpgsqlOptionsExtension is null");
        }

        var b = extension.WithConnectionString(connectionString);
#pragma warning restore EF1001
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(b);
    }

    [GeneratedRegex("^I[A-Za-z0-9_]+(Repository)$")]
    private static partial Regex RepositoryRegex();
}
