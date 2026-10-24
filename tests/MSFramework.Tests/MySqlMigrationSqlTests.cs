using System;
using System.Linq;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace MSFramework.Tests;

public sealed class MySqlMigrationSqlTests
{
    private sealed class MigrationContext(DbContextOptions<MigrationContext> options) : DbContext(options);

    [Fact]
    public void Load_NormalCreateTableMigrationRetainsForeignKeySql()
    {
        const string connectionString = "Server=localhost;Database=migrations;Uid=test;Pwd=test";
        var settings = new DbContextSettings
        {
            DatabaseType = "MySql",
            ConnectionString = connectionString
        };
        var options = new DbContextOptionsBuilder<MigrationContext>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)), mysql => mysql.Load(settings))
            .Options;
        using var context = new MigrationContext(options);
        var generator = context.GetService<IMigrationsSqlGenerator>();
        var parent = CreateTable("Parents");
        var child = CreateTable("Children");
        child.Columns.Add(new AddColumnOperation
        {
            Name = "ParentId",
            Table = child.Name,
            ClrType = typeof(int),
            ColumnType = "int",
            IsNullable = false
        });
        child.ForeignKeys.Add(new AddForeignKeyOperation
        {
            Name = "FK_Children_Parents_ParentId",
            Table = child.Name,
            Columns = ["ParentId"],
            PrincipalTable = parent.Name,
            PrincipalColumns = ["Id"]
        });

        var sql = string.Join('\n', generator.Generate([parent, child]).Select(command => command.CommandText));

        Assert.Contains("FOREIGN KEY", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FK_Children_Parents_ParentId", sql, StringComparison.Ordinal);
    }

    private static CreateTableOperation CreateTable(string name)
    {
        var operation = new CreateTableOperation { Name = name };
        operation.Columns.Add(new AddColumnOperation
        {
            Name = "Id",
            Table = name,
            ClrType = typeof(int),
            ColumnType = "int",
            IsNullable = false
        });
        operation.PrimaryKey = new AddPrimaryKeyOperation
        {
            Name = $"PK_{name}",
            Table = name,
            Columns = ["Id"]
        };
        return operation;
    }
}
