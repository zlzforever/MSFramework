using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;

public class TestDataContext(
    DbContextOptions options)
    : DbContextBase(options)
{
    protected override void ApplyConfiguration(ModelBuilder modelBuilder)
    {
    }
}
