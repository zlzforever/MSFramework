using System;
using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
/// 实体类配置类型查找器
/// </summary>
internal sealed class EntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
{
    private static readonly Dictionary<Type, Dictionary<Type, EntityTypeConfigurationMetadata>>
        EntityRegistersDict;

    private static readonly Dictionary<Type, Type> EntityMapDbContextDict;
    private static readonly Dictionary<Type, EntityTypeConfigurationMetadata> Empty;
    private static readonly HashSet<Type> DbContextTypes;

    static EntityConfigurationTypeFinder()
    {
        EntityRegistersDict = new Dictionary<Type, Dictionary<Type, EntityTypeConfigurationMetadata>>();
        EntityMapDbContextDict = new Dictionary<Type, Type>();
        Empty = new Dictionary<Type, EntityTypeConfigurationMetadata>();
        DbContextTypes = new HashSet<Type>();

        var assemblies = Utils.Runtime.GetAllAssemblies();

        var types = assemblies.SelectMany(assembly => assembly.DefinedTypes).Where(type =>
            type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition).ToArray();

        var applyConfigurationMethod = typeof(ModelBuilder)
            .GetMethods()
            .Single(
                e => e.Name == "ApplyConfiguration"
                     && e.ContainsGenericParameters
                     && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition()
                     == typeof(IEntityTypeConfiguration<>));

        foreach (var constructableType in types)
        {
            if (constructableType.GetConstructor(Type.EmptyTypes) == null)
            {
                continue;
            }

            object configuration = null;
            foreach (var type in constructableType.GetInterfaces())
            {
                if (!type.IsGenericType)
                {
                    continue;
                }

                // 只有指定了 DbContext 的接口才知道如何加载
                // 没有指定 DbContext 的只能靠用户自己处理
                if (type.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<,>))
                {
                    var entityType = type.GetGenericArguments()[0];
                    var dbContextType = type.GetGenericArguments()[1];
                    if (!EntityRegistersDict.ContainsKey(dbContextType))
                    {
                        EntityRegistersDict.Add(dbContextType,
                            new Dictionary<Type, EntityTypeConfigurationMetadata>());
                    }

                    configuration ??= Activator.CreateInstance(constructableType);

                    if (EntityRegistersDict[dbContextType].ContainsKey(entityType))
                    {
                        throw new MicroserviceFrameworkException($"类型 {entityType}, {dbContextType} 已经被注册过");
                    }

                    var methodInfo = applyConfigurationMethod.MakeGenericMethod(entityType);
                    EntityRegistersDict[dbContextType].Add(entityType,
                        new EntityTypeConfigurationMetadata(entityType, methodInfo, configuration));
                    EntityMapDbContextDict.AddOrUpdate(entityType, dbContextType);
                    DbContextTypes.Add(dbContextType);
                }

                // else if (type.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                // {
                // 	var entityType = type.GetGenericArguments()[0];
                // 	var dbContextType = typeof(DefaultDbContext);
                // 	if (!EntityRegistersDict.ContainsKey(dbContextType))
                // 	{
                // 		EntityRegistersDict.Add(dbContextType, new List<object>());
                // 	}
                //
                // 	EntityRegistersDict[dbContextType].Add(Activator.CreateInstance(constructableType));
                //
                // 	EntityMapDbContextDict.AddOrUpdate(entityType, dbContextType);
                // }
            }
        }
    }

    /// <summary>
    /// 获取指定上下文类型的实体配置注册信息
    /// </summary>
    /// <param name="dbContextType">数据上下文类型</param>
    /// <returns></returns>
    public Dictionary<Type, EntityTypeConfigurationMetadata> GetEntityTypeConfigurations(Type dbContextType)
    {
        return EntityRegistersDict.TryGetValue(dbContextType, out var value)
            ? value
            : Empty;
    }

    /// <summary>
    /// 获取 实体类所属的数据上下文类
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <returns>数据上下文类型</returns>
    public Type GetDbContextTypeForEntity(Type entityType)
    {
        if (!EntityMapDbContextDict.ContainsKey(entityType))
        {
            throw new MicroserviceFrameworkException(
                "未发现任何数据上下文实体映射配置");
        }

        return EntityMapDbContextDict[entityType];
    }

    public IEnumerable<Type> GetAllDbContextTypes()
    {
        return DbContextTypes;
    }

    public bool HasDbContextForEntity<T>()
    {
        return EntityMapDbContextDict.ContainsKey(typeof(T));
    }
}
