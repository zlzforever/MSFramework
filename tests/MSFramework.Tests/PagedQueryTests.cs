using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Linq.Expression;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MSFramework.Tests;

public class PagedQueryTests
{
    class TestEntity
    {
        public int Id { get; set; }
    }

    class TestDto
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// 供 EF Core 异步查询路径测试使用的实体
    /// </summary>
    public class PagingEntity
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// 供 EF Core 异步查询路径测试使用的内存数据库上下文
    /// </summary>
    public class PagingDbContext(DbContextOptions<PagingDbContext> options) : DbContext(options)
    {
        public DbSet<PagingEntity> Entities => Set<PagingEntity>();
    }

    [Fact]
    async Task PagedQueryAsync_ReturnsCorrectPaginationResult()
    {
        // Arrange
        var data = Enumerable.Range(1, 50)
            .Select(i => new TestEntity { Id = i }).AsQueryable();
        var page = 2;
        var limit = 10;

        // Act
        var result = await data.PagedQueryAsync(page, limit);

        // Assert
        Assert.Equal(page, result.Page);
        Assert.Equal(limit, result.Limit);
        Assert.Equal(50, result.Total);
        Assert.Equal(10, result.Data.Count());
        Assert.Equal(11, result.Data.First().Id);
    }

    [Fact]
    async Task PagedQueryAsync_ReturnsEmptyResult_WhenNoData()
    {
        // Arrange
        var data = Enumerable.Empty<TestEntity>().AsQueryable();
        var page = 1;
        var limit = 10;

        // Act
        var result = await data.PagedQueryAsync(page, limit);

        // Assert
        Assert.Equal(page, result.Page);
        Assert.Equal(limit, result.Limit);
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Data);
    }

    [Fact]
    async Task PagedQueryAsync_Mapper_ReturnsCorrectPaginationResult()
    {
        // Arrange
        var data = Enumerable.Range(1, 50).Select(i => new TestEntity { Id = i }).AsQueryable();
        var page = 2;
        var limit = 10;

        // Act
        var result = await data.PagedQueryAsync(page, limit, e => new TestDto { Id = e.Id });

        // Assert
        Assert.Equal(page, result.Page);
        Assert.Equal(limit, result.Limit);
        Assert.Equal(50, result.Total);
        Assert.Equal(10, result.Data.Count());
        Assert.Equal(11, result.Data.First().Id);
    }

    [Fact]
    async Task PagedQueryAsync_Mapper_ReturnsEmptyResult_WhenNoData()
    {
        // Arrange
        var data = Enumerable.Empty<TestEntity>().AsQueryable();
        var page = 1;
        var limit = 10;

        // Act
        var result = await data.PagedQueryAsync(page, limit, e => new TestDto { Id = e.Id });

        // Assert
        Assert.Equal(page, result.Page);
        Assert.Equal(limit, result.Limit);
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Data);
    }

    [Fact]
    async Task PagedQueryAsync_Mapper_ThrowsArgumentNullException_WhenMapperIsNull()
    {
        // Arrange
        var data = Enumerable.Range(1, 50).Select(i => new TestEntity { Id = i }).AsQueryable();
        var page = 1;
        var limit = 10;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            data.PagedQueryAsync<TestEntity, TestDto>(page, limit, null));
    }

    [Fact]
    async Task PagedQueryAsync_WithEfCore_UsesAsyncProvider()
    {
        // 旧实现同步 Count/ToList 会阻塞数据库线程；新实现走 CountAsync/ToListAsync 异步路径
        var options = new DbContextOptionsBuilder<PagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new PagingDbContext(options);
        db.Entities.AddRange(Enumerable.Range(1, 50).Select(i => new PagingEntity { Id = i }));
        await db.SaveChangesAsync();

        var result = await db.Entities.PagedQueryAsync(2, 10);

        Assert.Equal(50, result.Total);
        Assert.Equal(10, result.Data.Count());
        Assert.Equal(11, result.Data.First().Id);
    }

    [Fact]
    async Task PagedQueryAsync_WithEfCore_Mapper_UsesAsyncProvider()
    {
        var options = new DbContextOptionsBuilder<PagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new PagingDbContext(options);
        db.Entities.AddRange(Enumerable.Range(1, 50).Select(i => new PagingEntity { Id = i }));
        await db.SaveChangesAsync();

        var result = await db.Entities.PagedQueryAsync(2, 10, e => new TestDto { Id = e.Id });

        Assert.Equal(50, result.Total);
        Assert.Equal(10, result.Data.Count());
        Assert.Equal(11, result.Data.First().Id);
    }

    [Fact]
    async Task PagedQueryAsync_ReturnsEmptyResult_WhenOffsetExceedsIntRange()
    {
        var data = Enumerable.Range(1, 50).Select(i => new TestEntity { Id = i }).AsQueryable();

        var result = await data.PagedQueryAsync(int.MaxValue, int.MaxValue);

        Assert.Equal(int.MaxValue, result.Page);
        Assert.Equal(int.MaxValue, result.Limit);
        Assert.Equal(50, result.Total);
        Assert.Empty(result.Data);
    }

    [Fact]
    async Task PagedQueryAsync_Mapper_ReturnsEmptyResult_WhenOffsetExceedsIntRange()
    {
        var data = Enumerable.Range(1, 50).Select(i => new TestEntity { Id = i }).AsQueryable();

        var result = await data.PagedQueryAsync(int.MaxValue, int.MaxValue,
            entity => new TestDto { Id = entity.Id });

        Assert.Equal(50, result.Total);
        Assert.Empty(result.Data);
    }
}
