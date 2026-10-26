using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Linq.Expression;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MSFramework.Tests;

/// <summary>隔离修改进程级分页配置的测试，禁止与其他测试集合并行运行。</summary>
[CollectionDefinition(nameof(PagingDefaultsTests), DisableParallelization = true)]
public class PagingDefaultsCollection;

/// <summary>验证全局分页上限的设置与两个分页重载，不依赖外部数据库。</summary>
[Collection(nameof(PagingDefaultsTests))]
public class PagingDefaultsTests
{
    /// <summary>实体与 DTO 分页均遵守默认或自定义上限，包括无效数量回退和 EF 异步查询路径。</summary>
    /// <param name="maxLimit">正数分页上限。</param>
    /// <param name="requestedLimit">调用方请求的每页数量。</param>
    /// <param name="expectedLimit">归一化后的每页数量。</param>
    /// <param name="useEf">是否使用 EF 内存提供器验证异步查询。</param>
    [Theory]
    [InlineData(500, int.MaxValue, 500, false)]
    [InlineData(1000, int.MaxValue, 1000, false)]
    [InlineData(3, 0, 3, false)]
    [InlineData(3, -1, 3, false)]
    [InlineData(500, int.MaxValue, 500, true)]
    [InlineData(1000, int.MaxValue, 1000, true)]
    [InlineData(3, 0, 3, true)]
    [InlineData(3, -1, 3, true)]
    public async Task BothOverloadsRespectConfiguredLimit(int maxLimit, int requestedLimit, int expectedLimit, bool useEf)
    {
        var original = Defaults.MaxLimit;
        try
        {
            Defaults.MaxLimit = maxLimit;
            var options = new DbContextOptionsBuilder<PagingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            await using var context = new PagingContext(options);
            var entities = Enumerable.Range(1, 1100).Select(id => new PagingEntity { Id = id }).ToArray();
            IQueryable<PagingEntity> query = entities.AsQueryable();
            if (useEf)
            {
                context.AddRange(entities);
                await context.SaveChangesAsync();
                query = context.Set<PagingEntity>();
            }

            // 两个重载必须返回相同数量；不允许无效 limit 的默认值绕过自定义上限。
            query = query.OrderBy(entity => entity.Id);
            var result = await query.PagedQueryAsync(1, requestedLimit);
            var mapped = await query.PagedQueryAsync(1, requestedLimit, entity => entity.Id);
            Assert.Equal(expectedLimit, result.Limit);
            Assert.Equal(expectedLimit, mapped.Limit);
            Assert.Equal(expectedLimit, result.Data.Count());
            Assert.Equal(result.Data.Select(entity => entity.Id), mapped.Data);
            Assert.Equal(1100, result.Total);
            Assert.Equal(result.Total, mapped.Total);
        }
        finally
        {
            Defaults.MaxLimit = original;
        }
    }

    /// <summary>零或负数上限必须立即拒绝，原配置值保持不变。</summary>
    /// <param name="value">无效的全局分页上限。</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidLimitDoesNotChangeConfiguration(int value)
    {
        var original = Defaults.MaxLimit;
        Assert.Throws<ArgumentOutOfRangeException>(() => Defaults.MaxLimit = value);
        Assert.Equal(original, Defaults.MaxLimit);
    }

    private sealed class PagingEntity
    {
        /// <summary>测试记录的排序主键。</summary>
        public int Id { get; set; }
    }

    private sealed class PagingContext(DbContextOptions<PagingContext> options) : DbContext(options)
    {
        /// <summary>显式注册测试实体，供 EF 内存查询使用。</summary>
        /// <param name="modelBuilder">模型构建器，不允许为空。</param>
        /// <exception cref="ArgumentNullException">构建器为空。</exception>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.Entity<PagingEntity>();
        }
    }
}
