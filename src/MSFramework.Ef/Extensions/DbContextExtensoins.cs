#if NETSTANDARD2_1
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Extensions
{
	public static class DbContextExtensoins
	{
 
		public static string GetTableName<TEntity>(this DbContext dbContext) where TEntity : class
		{
			var dbSet = dbContext.Set<TEntity>();
			return dbSet.EntityType.GetTableName();
		}
	}
}
#endif