using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// 关系构建器扩展方法
/// </summary>
public static class ReferenceCollectionBuilderExtensions
{
    /// <summary>
    /// 配置"父聚合根集合移除子项时，子项转为软删除"的关系：
    /// <list type="bullet">
    /// <item>关系必须为必需（<c>IsRequired</c>）：子项外键不允许为空。若关系可选，EF Core 在关系切断时
    ///     会把外键置空（UPDATE 外键 = NULL），导致父子对应关系丢失；</item>
    /// <item>删除行为必须为 <see cref="DeleteBehavior.ClientCascade"/>：EF Core 会把被移除的子项标记为
    ///     <see cref="EntityState.Deleted"/>（外键值保留），由 <c>DeletionAuditingStrategy</c> 在保存时
    ///     转换为软删除 UPDATE；数据库外键不生成级联删除（避免真实 DELETE 扩散）。</item>
    /// </list>
    /// 子实体类型必须实现 <see cref="MicroserviceFramework.Domain.IDeletion"/>，
    /// 否则被移除的子项会被物理删除。
    /// </summary>
    /// <param name="builder">关系构建器</param>
    /// <typeparam name="TEntity">主体（聚合根）实体类型</typeparam>
    /// <typeparam name="TRelatedEntity">依赖（子项）实体类型</typeparam>
    /// <returns>关系构建器</returns>
    public static ReferenceCollectionBuilder<TEntity, TRelatedEntity> OnSoftDelete<TEntity, TRelatedEntity>(
        this ReferenceCollectionBuilder<TEntity, TRelatedEntity> builder) where TEntity : class
        where TRelatedEntity : class
    {
        builder.IsRequired();
        builder.OnDelete(DeleteBehavior.ClientCascade);
        builder.Metadata.AddAnnotation("OnSoftDelete", true);
        return builder;
    }
}
