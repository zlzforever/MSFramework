using System;
using System.Threading.Tasks;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Domain
{
	// /// <summary>
	// /// Just to mark a class as repository.
	// /// </summary>
	// public interface IRepository
	// {
	// }

	public interface IRepository<TEntity> : IRepository<TEntity, ObjectId>
		where TEntity : IAggregateRoot<ObjectId>
	{
	}

	public interface IRepository<TEntity, in TKey>
		where TEntity : IAggregateRoot<TKey> where TKey : IEquatable<TKey>
	{
		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		TEntity Find(TKey id);

		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		Task<TEntity> FindAsync(TKey id);

		/// <summary>
		/// Inserts a new entity.
		/// </summary>
		/// <param name="entity">Inserted entity</param>
		void Add(TEntity entity);

		/// <summary>
		/// Inserts a new entity.
		/// </summary>
		/// <param name="entity">Inserted entity</param>
		Task AddAsync(TEntity entity);

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