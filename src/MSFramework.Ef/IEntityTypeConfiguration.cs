using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef;

// ReSharper disable once UnusedTypeParameter
public interface IEntityTypeConfiguration<TEntity, TDbContext> :
    IEntityTypeConfiguration<TEntity>, IExternalMeta
    where TEntity : class where TDbContext : DbContext
{
}

public interface IExternalMeta
{
    bool IsExternal { get; }
}
