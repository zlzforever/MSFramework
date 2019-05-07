using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.DependencyInjection;

namespace MSFramework.Domain.Repository
{
	/// <summary>
	/// This interface must be implemented by all repositories to identify them by convention.
	/// Implement generic version instead of this one.
	/// </summary>
	public interface IRepository : IScopeDependency
	{
	}

	public interface IRepository<TAggregateRoot, in TAggregateRootId> : IRepository
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
		/// <summary>
		/// Used to get all entities.
		/// </summary>
		/// <returns>List of all entities</returns>
		List<TAggregateRoot> GetAllList();

		/// <summary>
		/// Used to get all entities.
		/// </summary>
		/// <returns>List of all entities</returns>
		Task<List<TAggregateRoot>> GetAllListAsync();

		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		TAggregateRoot Get(TAggregateRootId id);

		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		Task<TAggregateRoot> GetAsync(TAggregateRootId id);

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

		/// <summary>
		/// Inserts a new entity.
		/// </summary>
		/// <param name="entity">Inserted entity</param>
		TAggregateRoot Insert(TAggregateRoot entity);

		/// <summary>
		/// Inserts a new entity.
		/// </summary>
		/// <param name="entity">Inserted entity</param>
		Task<TAggregateRoot> InsertAsync(TAggregateRoot entity);

		/// <summary>
		/// Updates an existing entity.
		/// </summary>
		/// <param name="entity">Entity</param>
		TAggregateRoot Update(TAggregateRoot entity);

		/// <summary>
		/// Updates an existing entity. 
		/// </summary>
		/// <param name="entity">Entity</param>
		Task<TAggregateRoot> UpdateAsync(TAggregateRoot entity);

		/// <summary>
		/// Deletes an entity.
		/// </summary>
		/// <param name="entity">Entity to be deleted</param>
		void Delete(TAggregateRoot entity);

		/// <summary>
		/// Deletes an entity.
		/// </summary>
		/// <param name="entity">Entity to be deleted</param>
		Task DeleteAsync(TAggregateRoot entity);

		/// <summary>
		/// Deletes an entity by primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity</param>
		void Delete(TAggregateRootId id);

		/// <summary>
		/// Deletes an entity by primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity</param>
		Task DeleteAsync(TAggregateRootId id);
	}
}