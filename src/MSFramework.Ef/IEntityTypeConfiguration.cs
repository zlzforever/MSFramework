using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef;

/// <summary>
///
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TDbContext"></typeparam>
public interface IEntityTypeConfiguration<TEntity, TDbContext> :
    IEntityTypeConfiguration,
    IEntityTypeConfiguration<TEntity>
    where TEntity : class where TDbContext : DbContext;

/// <summary>
///
/// </summary>
public interface IEntityTypeConfiguration
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="modelBuilder"></param>
    void Configure(ModelBuilder modelBuilder);
}
