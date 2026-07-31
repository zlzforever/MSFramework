using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories;

/// <summary>
/// 无键 EF Core 仓储基类，面向复合主键（多属性作主键、无 Id 包装）的聚合根。
/// <para>
/// 为兑现非泛型 <see cref="IAggregateRoot"/> 接口「主键可能不是 Id 或为复合主键」的设计初衷而提供：
/// 聚合根以多个标量属性直接作为主键（无 Id 包装）时，无法使用 <see cref="EfRepository{TEntity,TKey}"/>，
/// 应继承本基类并通过表达式谓词（<see cref="FindAsync(System.Linq.Expressions.Expression{Func{TAggregateRoot,bool}},System.Threading.CancellationToken)"/>）
/// 完成查询。<see cref="EfRepository{TEntity,TKey}"/> 继承自本类，复用 Store/查询/审计逻辑。
/// </para>
/// </summary>
/// <typeparam name="TAggregateRoot">聚合根类型，需实现非泛型 <see cref="IAggregateRoot"/></typeparam>
public class EfRepository<TAggregateRoot> : IRepository<TAggregateRoot>, IEfRepository
    where TAggregateRoot : class, IAggregateRoot
{
    private readonly DbSet<TAggregateRoot> _dbSet;
    private readonly DbContextBase _dbContext;

    /// <summary>
    /// 获取可查询的实体集合，默认包含第一级导航属性
    /// </summary>
    protected virtual IQueryable<TAggregateRoot> Store
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            var queryable = BuildQueryable(_dbSet);
            field = queryable;

            return field;
        }
    }

    /// <summary>
    /// 获取原始 DbSet 实例
    /// </summary>
    protected DbSet<TAggregateRoot> DbSet => _dbSet;

    /// <summary>
    /// 获取当前 DbContext 实例
    /// </summary>
    public DbContext DbContext => _dbContext;

    /// <summary>
    /// 获取或设置是否启用查询拆分行为，null 时使用全局设置
    /// </summary>
    public bool? UseQuerySplittingBehavior { get; init; }

    /// <summary>
    /// 初始化 EfRepository 实例
    /// </summary>
    /// <param name="dbContextFactory">数据库上下文工厂</param>
    public EfRepository(DbContextFactory dbContextFactory)
    {
        _dbContext = dbContextFactory.GetDbContext<TAggregateRoot>();
        _dbSet = _dbContext.Set<TAggregateRoot>();
    }

    /// <summary>
    /// 若 UseQuerySplittingBehavior 为空，则使用全局设置，默认是 SingleQuery
    /// 建议 2 个或以上的 1:N 关系则使用 SplitQuery 来避免笛卡尔积爆炸，其它情况使用 SingleQuery
    /// 即其他情况使用默认配置，仅在聚合根有较多 1:N 的关系时重载 UseQuerySplittingBehavior = true 来优化查询
    /// 默认会 include 第一级导航属性
    /// </summary>
    /// <param name="dbSet"></param>
    /// <returns></returns>
    protected virtual IQueryable<TAggregateRoot> BuildQueryable(DbSet<TAggregateRoot> dbSet)
    {
        var queryable = dbSet.AsQueryable();
        var navigations = dbSet.EntityType.GetNavigations();
        queryable = navigations.Aggregate(queryable, (current, navigation) => current.Include(navigation.Name));

        return !UseQuerySplittingBehavior.HasValue ? queryable :
            UseQuerySplittingBehavior.Value ? queryable.AsSplitQuery() : queryable.AsSingleQuery();
    }

    /// <summary>
    /// 获取可查询的聚合根集合，默认包含第一级导航属性
    /// </summary>
    /// <returns>聚合根查询对象</returns>
    public virtual IQueryable<TAggregateRoot> GetQueryable()
    {
        return Store;
    }

    /// <summary>
    /// 异步获取可查询的聚合根集合，默认包含第一级导航属性
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聚合根查询对象</returns>
    public virtual Task<IQueryable<TAggregateRoot>> GetQueryableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Store);
    }

    /// <summary>
    /// 通过表达式谓词查找聚合根。
    /// 复合主键实体无单一 TKey，须通过成员等值谓词（如 <c>x =&gt; x.OrderId == orderId &amp;&amp; x.ProductId == productId</c>）定位；
    /// 对实现 <see cref="IDeletion"/> 的实体，已软删除的记录视为不存在（返回 null），与既有全局查询过滤器行为保持一致。
    /// </summary>
    /// <param name="predicate">查询谓词，用于定位聚合根</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配的聚合根，未找到或已软删除则返回 null</returns>
    public virtual async Task<TAggregateRoot> FindAsync(
        Expression<Func<TAggregateRoot, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await Store.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// 添加新实体到仓储
    /// </summary>
    /// <param name="entity">要添加的实体</param>
    public virtual void Add(TAggregateRoot entity)
    {
        _dbSet.Add(entity);
    }

    /// <summary>
    /// 异步添加新实体到仓储
    /// </summary>
    /// <param name="entity">要添加的实体</param>
    public virtual async Task AddAsync(TAggregateRoot entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// 从仓储中删除实体
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    public virtual void Delete(TAggregateRoot entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// 异步从仓储中删除实体
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    /// <returns>异步任务</returns>
    public virtual Task DeleteAsync(TAggregateRoot entity)
    {
        Delete(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取原始 DbSet 实例，用于高级查询操作
    /// </summary>
    /// <returns>DbSet 实例</returns>
    public DbSet<TAggregateRoot> GetDbSet()
    {
        return _dbSet;
    }
}

/// <summary>
/// EF Core 仓储基类，提供聚合根的增删改查基础实现
/// </summary>
/// <typeparam name="TEntity">聚合根实体类型</typeparam>
/// <typeparam name="TKey">实体主键类型</typeparam>
public class EfRepository<TEntity, TKey> : EfRepository<TEntity>, IRepository<TEntity, TKey>
    where TEntity : class, IAggregateRoot<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 初始化 EfRepository 实例
    /// </summary>
    /// <param name="dbContextFactory">数据库上下文工厂</param>
    public EfRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
    {
    }

    /// <summary>
    /// 根据主键查找实体。
    /// 经 <c>Store</c>（默认包含第一级导航属性）以主键等值谓词查询，支持单键与复合键值对象（通过值转换器映射）两种场景；
    /// 对实现 <see cref="IDeletion"/> 的实体，已软删除的记录视为不存在（返回 null），与既有全局查询过滤器行为保持一致。
    /// </summary>
    /// <param name="id">实体主键（单键或复合键值对象）；传 null 时返回 null 不抛异常</param>
    /// <returns>匹配的实体，未找到或已软删除则返回 null</returns>
    public virtual TEntity Find(TKey id)
    {
        var entity = Store.FirstOrDefault(x => x.Id.Equals(id));
        return entity is IDeletion { IsDeleted: true } ? null : entity;
    }

    /// <summary>
    /// 异步根据主键查找实体。
    /// 经 <c>Store</c>（默认包含第一级导航属性）以主键等值谓词查询，支持单键与复合键值对象（通过值转换器映射）两种场景；
    /// 对实现 <see cref="IDeletion"/> 的实体，已软删除的记录视为不存在（返回 null），与既有全局查询过滤器行为保持一致。
    /// </summary>
    /// <param name="id">实体主键（单键或复合键值对象）；传 null 时返回 null 不抛异常</param>
    /// <returns>匹配的实体，未找到或已软删除则返回 null</returns>
    public virtual async Task<TEntity> FindAsync(TKey id)
    {
        var entity = await Store.FirstOrDefaultAsync(x => x.Id.Equals(id));
        return entity is IDeletion { IsDeleted: true } ? null : entity;
    }

    /// <summary>
    /// 根据主键删除实体
    /// </summary>
    /// <param name="id">要删除的实体主键</param>
    public virtual void Delete(TKey id)
    {
        var entity = Find(id);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    /// <summary>
    /// 异步根据主键删除实体
    /// </summary>
    /// <param name="id">要删除的实体主键</param>
    public virtual async Task DeleteAsync(TKey id)
    {
        var entity = await FindAsync(id);
        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }
}
