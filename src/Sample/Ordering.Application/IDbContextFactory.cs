using Microsoft.EntityFrameworkCore;

namespace Ordering.Application;

public interface IDbContextFactory
{
    DbContext GetDbContext<TEntity>();
}
