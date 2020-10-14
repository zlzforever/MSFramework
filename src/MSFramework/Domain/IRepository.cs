using System.Threading.Tasks;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Domain
{
	/// <summary>
	/// Just to mark a class as repository.
	/// </summary>
	public interface IRepository
	{
	}

	public interface IRepository<TEntity> : IRepository<TEntity, ObjectId>
		where TEntity : IAggregateRoot
	{
	}

	public interface IRepository<TEntity, in TKey> : IRepository
		where TEntity : IAggregateRoot
	{
		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		TEntity Get(TKey id);

		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		Task<TEntity> GetAsync(TKey id);

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
		void Delete(TKey id);

		/// <summary>
		/// Deletes an entity by primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity</param>
		Task DeleteAsync(TKey id);
	}
}