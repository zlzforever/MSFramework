using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef;

public interface IEntityTypeConfiguration<TEntity, TDbContext> :
    IEntityTypeConfiguration<TEntity>
    where TEntity : class where TDbContext : DbContext;
