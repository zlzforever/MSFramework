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
    private readonly Dictionary<Type, List<IEntityTypeConfiguration>> _dbContextConfigs = new();
    private readonly Dictionary<Type, Type> _entityToDbContext = new();
    private readonly HashSet<Type> _dbContextTypes = [];

    /// <summary>
    /// 构造时完成程序集扫描与配置实例化。
    /// 使用实例字段（而非静态构造函数）保存结果：配置错误时直接抛出原始
    /// <see cref="MicroserviceFrameworkException"/>，不会被 CLR 包装成
    /// <see cref="TypeInitializationException"/> 并在进程内永久缓存失败状态。
    /// </summary>
    /// <exception cref="MicroserviceFrameworkException">实体配置类缺少公共无参构造、
    /// 同一配置类型重复注册，或同一实体映射到多个 DbContext 时抛出</exception>
    public EntityConfigurationTypeFinder()
    {
        var types = Utils.Runtime.GetAllAssemblies()
            .SelectMany(a => a.DefinedTypes)
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

        foreach (var type in types)
        {
            // 先判定"是否为实体配置类"再校验构造函数：
            // 顺序颠倒会对所有无参构造缺失的普通类型误报
            var (entityType, dbContextType) = GetEntityConfigTypeArgs(type);
            if (entityType == null)
            {
                continue;
            }

            // 显式失败优于静默跳过：框架鼓励 primary constructor 风格的 DI，
            // 配置类一旦写成 "public class X(ISomething s) : EntityTypeConfigurationBase<...>"，
            // 旧实现会静默 continue——直到运行期报"实体不在模型中"，报错信息与真实根因
            // （配置类被跳过）完全对不上。IEntityTypeConfiguration 在模型构建期以无参形式
            // 实例化，不参与依赖注入，因此必须提供公共无参构造
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new MicroserviceFrameworkException(
                    $"实体配置类型 {type.FullName} 缺少公共无参构造。" +
                    "IEntityTypeConfiguration 在模型构建期以无参形式实例化（不参与依赖注入），" +
                    "请去掉构造函数参数；依赖请通过 ApplyConfiguration 内的 GetService 解析");
            }

            if (!_dbContextConfigs.TryGetValue(dbContextType, out var list))
            {
                list = [];
                _dbContextConfigs[dbContextType] = list;
            }

            if (list.Any(x => x.GetType() == type))
            {
                throw new MicroserviceFrameworkException(
                    $"类型 {entityType} 在 {dbContextType} 中已注册");
            }

            list.Add((IEntityTypeConfiguration)Activator.CreateInstance(type));

            // 同一实体必须且只能映射到一个 DbContext（框架约定）：发现冲突映射时直接抛异常，
            // 而非静默忽略——TryAdd 静默失败会让后注册上下文的配置沦为死代码，
            // 仓储读写按先注册者落库，错误直到读错数据时才暴露，排查成本极高
            if (!_entityToDbContext.TryAdd(entityType, dbContextType))
            {
                throw new MicroserviceFrameworkException(
                    $"实体 {entityType.FullName} 已映射到上下文 {_entityToDbContext[entityType].FullName}，" +
                    "同一实体只能映射到一个 DbContext；当前冲突配置类型：" +
                    $"{type.FullName}（目标上下文 {dbContextType.FullName}）");
            }

            _dbContextTypes.Add(dbContextType);
        }

        LogRegisteredEntities();
    }

    /// <summary>按上下文汇总已注册实体，每个上下文占一行，实体名称使用竖线分隔。</summary>
    private void LogRegisteredEntities()
    {
        if (_dbContextConfigs.Count == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("实体配置注册完成:");

        foreach (var (dbContextType, configs) in _dbContextConfigs)
        {
            sb.AppendLine($"  [{dbContextType.Name}] {string.Join(" | ", configs.Select(config => config.GetEntityType().Name))}");
        }

        Defaults.Logger?.LogInformation(sb.ToString());
    }

    public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType)
    {
        return _dbContextConfigs.TryGetValue(dbContextType, out var list)
            ? list
            : Enumerable.Empty<IEntityTypeConfiguration>();
    }

    public Type GetDbContextTypeForEntity(Type entityType)
    {
        return _entityToDbContext.TryGetValue(entityType, out var dbContextType)
            ? dbContextType
            : throw new MicroserviceFrameworkException(
                $"实体 {entityType.FullName} 未注册到任何 DbContext。" +
                "请为其编写 EntityTypeConfigurationBase<" + entityType.Name + ", TContext> 配置类" +
                "（须提供公共无参构造）");
    }

    public IEnumerable<Type> GetAllDbContextTypes() => _dbContextTypes;

    public bool HasDbContextForEntity<T>() => _entityToDbContext.ContainsKey(typeof(T));

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
