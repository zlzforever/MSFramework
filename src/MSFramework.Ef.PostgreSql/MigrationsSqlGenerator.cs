using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace MicroserviceFramework.Ef.PostgreSql;

public class MigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
{
    public MigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies,
        INpgsqlSingletonOptions npgsqlSingletonOptions) : base(dependencies, npgsqlSingletonOptions)
    {
    }
}
