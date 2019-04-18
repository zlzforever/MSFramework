using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.Domain.Entity;

namespace MSFramework.Domain.Repository
{
	/// <summary>
	/// This interface is implemented by all repositories to ensure implementation of fixed methods.
	/// </summary>
	/// <typeparam name="TEntity">Main Entity type this repository works on</typeparam>
	/// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
	public interface IRepository<TEntity, in TPrimaryKey> : IRepository where TEntity : class, IEntity<TPrimaryKey>
	{
		#region Select/Get/Query

		/// <summary>
		/// Used to get all entities.
		/// </summary>
		/// <returns>List of all entities</returns>
		List<TEntity> GetAllList();

		/// <summary>
		/// Used to get all entities.
		/// </summary>
		/// <returns>List of all entities</returns>
		Task<List<TEntity>> GetAllListAsync();

		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		TEntity Get(TPrimaryKey id);

		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		Task<TEntity> GetAsync(TPrimaryKey id);

		#endregion

		#region Insert

		/// <summary>
		/// Inserts a new entity.
		/// </summary>
		/// <param name="entity">Inserted entity</param>
		TEntity Insert(TEntity entity);

		/// <summary>
		/// Inserts a new entity.
		/// </summary>
		/// <param name="entity">Inserted entity</param>
		Task<TEntity> InsertAsync(TEntity entity);

		#endregion

		#region Update

		/// <summary>
		/// Updates an existing entity.
		/// </summary>
		/// <param name="entity">Entity</param>
		TEntity Update(TEntity entity);

		/// <summary>
		/// Updates an existing entity. 
		/// </summary>
		/// <param name="entity">Entity</param>
		Task<TEntity> UpdateAsync(TEntity entity);

		#endregion

		#region Delete

		/// <summary>
		/// Deletes an entity.
		/// </summary>
		/// <param name="entity">Entity to be deleted</param>
		void Delete(TEntity entity);

		/// <summary>
		/// Deletes an entity.
		/// </summary>
		/// <param name="entity">Entity to be deleted</param>
		Task DeleteAsync(TEntity entity);

		/// <summary>
		/// Deletes an entity by primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity</param>
		void Delete(TPrimaryKey id);

		/// <summary>
		/// Deletes an entity by primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity</param>
		Task DeleteAsync(TPrimaryKey id);

		#endregion

		#region Aggregates

		/// <summary>
		/// Gets count of all entities in this repository.
		/// </summary>
		/// <returns>Count of entities</returns>
		int Count();

		/// <summary>
		/// Gets count of all entities in this repository.
		/// </summary>
		/// <returns>Count of entities</returns>
		Task<int> CountAsync();

		/// <summary>
		/// Gets count of all entities in this repository (use if expected return value is greather than <see cref="int.MaxValue"/>.
		/// </summary>
		/// <returns>Count of entities</returns>
		long LongCount();

		/// <summary>
		/// Gets count of all entities in this repository (use if expected return value is greather than <see cref="int.MaxValue"/>.
		/// </summary>
		/// <returns>Count of entities</returns>
		Task<long> LongCountAsync();

		#endregion
	}
}