using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace MSFramework.Tests;

public class CreateTableOperationTests
{
    [Fact]
    public void A()
    {
        var operation = new CreateTableOperation { Name = "test" };
        var b = operation.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
        var b11 = typeof(ITableMigrationOperation).GetProperties();
    }
}
