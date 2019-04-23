using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Domain;
using MSFramework.Domain.Repository;

namespace MSFramework.EntityFrameworkCore.Repository
{
	/// <summary>
	/// This interface is implemented by all repositories to ensure implementation of fixed methods.
	/// </summary>
	/// <typeparam name="TEntity">Main Entity type this repository works on</typeparam>
	/// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
	public interface IEfRepository<TEntity, in TPrimaryKey> :
		IRepository<TEntity, TPrimaryKey>
		where TEntity : class, IEntity<TPrimaryKey>
	{
		DbContext Context { get; }
		
		DbConnection Connection { get; }
		
		DbSet<TEntity> Table { get; }
		
		#region Select/Get/Query

		/// <summary>
		/// Used to get a IQueryable that is used to retrieve entities from entire table.
		/// </summary>
		/// <returns>IQueryable to be used to select entities from database</returns>
		IQueryable<TEntity> GetAll();

		IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors);

		/// <summary>
		/// Used to get all entities based on given <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A condition to filter entities</param>
		/// <returns>List of all entities</returns>
		List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Used to get all entities based on given <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A condition to filter entities</param>
		/// <returns>List of all entities</returns>
		Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate);

		Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		///     Gets the Entity with specified predicate
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

		#endregion

		#region Delete

		/// <summary>
		/// Deletes many entities by function.
		/// Notice that: All entities fits to given predicate are retrieved and deleted.
		/// This may cause major performance problems if there are too many entities with
		/// given predicate.
		/// </summary>
		/// <param name="predicate">A condition to filter entities</param>
		void Delete(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Deletes many entities by function.
		/// Notice that: All entities fits to given predicate are retrieved and deleted.
		/// This may cause major performance problems if there are too many entities with
		/// given predicate.
		/// </summary>
		/// <param name="predicate">A condition to filter entities</param>
		Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

		#endregion

		#region Aggregates

		/// <summary>
		/// Gets count of all entities in this repository based on given <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A method to filter count</param>
		/// <returns>Count of entities</returns>
		int Count(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Gets count of all entities in this repository based on given <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A method to filter count</param>
		/// <returns>Count of entities</returns>
		Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Gets count of all entities in this repository based on given <paramref name="predicate"/>
		/// (use this overload if expected return value is greather than <see cref="int.MaxValue"/>).
		/// </summary>
		/// <param name="predicate">A method to filter count</param>
		/// <returns>Count of entities</returns>
		long LongCount(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		/// Gets count of all entities in this repository based on given <paramref name="predicate"/>
		/// (use this overload if expected return value is greather than <see cref="int.MaxValue"/>).
		/// </summary>
		/// <param name="predicate">A method to filter count</param>
		/// <returns>Count of entities</returns>
		Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate);

		#endregion
	}
}