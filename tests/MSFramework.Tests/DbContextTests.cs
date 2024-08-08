using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MSFramework.Tests;

public class DbContextTests
{
    [Fact]
    public void GetTableName_ReturnsCorrectTableName_ForValidEntity()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        using var context = new TestDbContext(options);
        var tableName = context.GetTableName<TestEntity>();
        Assert.Equal("TestEntity", tableName);
    }

    [Fact]
    public void GetTableName_ReturnsNull_ForInvalidEntity()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        using var context = new TestDbContext(options);
        var tableName = context.GetTableName<NonExistentEntity>();
        Assert.Null(tableName);
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> TestEntities { get; set; }
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class NonExistentEntity
    {
        public int Id { get; set; }
    }
}
