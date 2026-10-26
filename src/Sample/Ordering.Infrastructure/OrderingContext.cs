using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure;

public class OrderingContext(
    DbContextOptions<OrderingContext> options)
    : DbContextBase(options)
{
    protected override void ApplyConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyAuditingConfiguration();
    }
}
