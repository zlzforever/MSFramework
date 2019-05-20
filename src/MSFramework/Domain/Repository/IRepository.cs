using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.DependencyInjection;

namespace MSFramework.Domain.Repository
{
	public interface IRepository<TEntity> : IScopeDependency
		where TEntity : IAggregateRoot
	{
		IUnitOfWork UnitOfWork { get; }

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
		TEntity Get(Guid id);

		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		Task<TEntity> GetAsync(Guid id);

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
		void Delete(Guid id);

		/// <summary>
		/// Deletes an entity by primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity</param>
		Task DeleteAsync(Guid id);
	}
}