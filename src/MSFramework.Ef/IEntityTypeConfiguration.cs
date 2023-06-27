using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef;

// ReSharper disable once UnusedTypeParameter
public interface IEntityTypeConfiguration<TEntity, TDbContext> :
    IEntityTypeConfiguration<TEntity>
    where TEntity : class where TDbContext : DbContext
{
}
