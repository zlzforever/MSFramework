using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
/// 启动时扫描所有程序集
/// 按 DbContext 类型分组缓存。
/// </summary>
internal sealed class EntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
{
    private static readonly Dictionary<Type, List<IEntityTypeConfiguration>> DbContextConfigs;
    private static readonly Dictionary<Type, Type> EntityToDbContext;
    private static readonly HashSet<Type> DbContextTypes;

    static EntityConfigurationTypeFinder()
    {
        DbContextConfigs = new Dictionary<Type, List<IEntityTypeConfiguration>>();
        EntityToDbContext = new Dictionary<Type, Type>();
        DbContextTypes = [];

        var types = Utils.Runtime.GetAllAssemblies()
            .SelectMany(a => a.DefinedTypes)
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

        foreach (var type in types)
        {
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                continue;
            }

            var (entityType, dbContextType) = GetEntityConfigTypeArgs(type);
            if (entityType == null)
            {
                continue;
            }

            if (!DbContextConfigs.TryGetValue(dbContextType, out var list))
            {
                list = [];
                DbContextConfigs[dbContextType] = list;
            }

            if (list.Any(x => x.GetType() == type))
            {
                throw new MicroserviceFrameworkException(
                    $"类型 {entityType} 在 {dbContextType} 中已注册");
            }

            list.Add((IEntityTypeConfiguration)Activator.CreateInstance(type));
            EntityToDbContext.TryAdd(entityType, dbContextType);
            DbContextTypes.Add(dbContextType);
        }

        LogRegisteredEntities();
    }

    private static void LogRegisteredEntities()
    {
        if (DbContextConfigs.Count == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("实体配置注册完成:");

        foreach (var (dbContextType, configs) in DbContextConfigs)
        {
            sb.AppendLine($"  [{dbContextType.Name}]");
            foreach (var config in configs)
            {
                sb.AppendLine($"    {config.GetEntityType().Name}");
            }
        }

        Defaults.Logger?.LogInformation(sb.ToString());
    }

    public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType)
    {
        return DbContextConfigs.TryGetValue(dbContextType, out var list)
            ? list
            : Enumerable.Empty<IEntityTypeConfiguration>();
    }

    public Type GetDbContextTypeForEntity(Type entityType)
    {
        return EntityToDbContext.TryGetValue(entityType, out var dbContextType)
            ? dbContextType
            : throw new MicroserviceFrameworkException("未发现任何数据库上下文实体映射配置");
    }

    public IEnumerable<Type> GetAllDbContextTypes() => DbContextTypes;

    public bool HasDbContextForEntity<T>() => EntityToDbContext.ContainsKey(typeof(T));

    private static (Type EntityType, Type DbContextType) GetEntityConfigTypeArgs(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(EntityTypeConfigurationBase<,>))
            {
                var args = baseType.GetGenericArguments();
                return (args[0], args[1]);
            }

            baseType = baseType.BaseType;
        }

        return (null, null);
    }
}
