using System;

namespace MicroserviceFramework.Domain;

/// <summary>
///
/// </summary>
public interface IExternalEntity;

/// <summary>
/// 系统外部实体
/// 可以配置到 Ef 中，但不会被 EF 管理，仅用于数据查询
/// 外部表仅仅是对象的添加、修改、删除不进行跟踪，但 Migrate 依赖会生成相关脚本。因此需要注释掉创建与删除表的代码！！！
/// 理论上外部表应该是已经创建了的，若没有注释掉创建脚本则会导致迁移失败（表已经存在）
/// 若没有注释掉删除表的脚本，在数据库降级操作可能导致表的删除！！！
/// </summary>
public abstract class ExternalEntity<TKey>(TKey id) : EntityBase<TKey>(id), IExternalEntity
    where TKey : IEquatable<TKey>;
