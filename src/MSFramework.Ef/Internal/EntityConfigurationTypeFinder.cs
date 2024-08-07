using System;
using System.Collections.Generic;
using System.Linq;
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
    // private static readonly Dictionary<string, Type> EntityNameMapEntityTypeDict;
    private static readonly HashSet<Type> DbContextTypes;

    static EntityConfigurationTypeFinder()
    {
        EntityRegistersDict = new Dictionary<Type, Dictionary<Type, EntityTypeConfigurationMetadata>>();
        EntityMapDbContextDict = new Dictionary<Type, Type>();
        // EntityNameMapEntityTypeDict = new();
        DbContextTypes = [];
        var createEntityTypeBuilderMethod = typeof(ModelBuilder)
            .GetMethods().First(x => x.Name == "Entity" && x.GetGenericArguments().Length == 1);

        var assemblies = Utils.Runtime.GetAllAssemblies();

        var types = assemblies.SelectMany(assembly => assembly.DefinedTypes).Where(type =>
            type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition);

        // var applyConfigurationMethod = typeof(ModelBuilder)
        //     .GetMethods()
        //     .Single(
        //         e => e.Name == "ApplyConfiguration"
        //              && e.ContainsGenericParameters
        //              && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition()
        //              == typeof(IEntityTypeConfiguration<>));

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

                    var dict = EntityRegistersDict[dbContextType];

                    if (dict.ContainsKey(entityType))
                    {
                        throw new MicroserviceFrameworkException($"类型 {entityType}, {dbContextType} 已经注册");
                    }

                    var configureMethodInfo = typeof(IEntityTypeConfiguration<>) // IEntityTypeConfiguration<TEntity>
                        .MakeGenericType(entityType)
                        .GetMethod("Configure");

                    configuration ??= Activator.CreateInstance(constructableType);
                    var metadata = new EntityTypeConfigurationMetadata(entityType, configureMethodInfo,
                        createEntityTypeBuilderMethod.MakeGenericMethod(entityType), configuration);
                    dict.Add(entityType, metadata);
                    EntityMapDbContextDict.TryAdd(entityType, dbContextType);
                    // EntityNameMapEntityTypeDict.TryAdd(entityType.FullName, entityType);
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

    // public Type GetEntityType(string name)
    // {
    //     return EntityNameMapEntityTypeDict.TryGetValue(name, out var value) ? value : null;
    // }

    /// <summary>
    /// 获取指定上下文类型的实体配置注册信息
    /// </summary>
    /// <param name="dbContextType">数据上下文类型</param>
    /// <returns></returns>
    public IEnumerable<EntityTypeConfigurationMetadata> GetEntityTypeConfigurations(Type dbContextType)
    {
        return EntityRegistersDict.TryGetValue(dbContextType, out var value)
            ? value.Values
            : Enumerable.Empty<EntityTypeConfigurationMetadata>();
    }

    /// <summary>
    /// 获取 实体类所属的数据上下文类
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <returns>数据上下文类型</returns>
    public Type GetDbContextTypeForEntity(Type entityType)
    {
        if (!EntityMapDbContextDict.TryGetValue(entityType, out var entity))
        {
            throw new MicroserviceFrameworkException("未发现任何数据库上下文实体映射配置");
        }

        return entity;
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
