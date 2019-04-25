using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSFramework.Domain.Repository
{
	public interface IReadRepository<TAggregateRoot, in TAggregateRootId> : IRepository
		where TAggregateRoot : AggregateRootBase<TAggregateRoot,TAggregateRootId>
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
	}
}