using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Linq.Expression;
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
}
