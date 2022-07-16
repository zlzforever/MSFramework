using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroserviceFramework.Ef
{
    /// <summary>
    /// 定义实体类配置类型查找器
    /// </summary>
    public interface IEntityConfigurationTypeFinder
    {
        /// <summary>
        /// 获取指定上下文类型的实体配置注册信息
        /// </summary>
        /// <param name="dbContextType">数据上下文类型</param>
        /// <returns></returns>
        Dictionary<Type, EntityTypeConfigurationMetadata> GetEntityTypeConfigurations(Type dbContextType);

        /// <summary>
        /// 获取 实体类所属的数据上下文类
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <returns>数据上下文类型</returns>
        Type GetDbContextTypeForEntity(Type entityType);

        IEnumerable<Type> GetAllDbContextTypes();

        /// <summary>
        /// 判断实体有无配置到 DbContext
        /// </summary>
        /// <returns></returns>
        bool HasDbContextForEntity<T>();
    }

    public struct EntityTypeConfigurationMetadata
    {
        public readonly Type EntityType;
        public readonly MethodInfo MethodInfo;
        public readonly object EntityTypeConfiguration;

        public EntityTypeConfigurationMetadata(Type entityType, MethodInfo methodInfo, object entityTypeConfiguration)
        {
            EntityType = entityType;
            MethodInfo = methodInfo;
            EntityTypeConfiguration = entityTypeConfiguration;
        }
    }
}