using System;
using System.Threading.Tasks;
using MSFramework.Domain;
using MSFramework.Domain.Repository;

namespace MSFramework.EntityFrameworkCore.Repository
{
	public interface IEfWriteRepository<TAggregateRoot>
		: IWriteRepository<TAggregateRoot>
		where TAggregateRoot : AggregateRootBase
	{
		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		TAggregateRoot Get(Guid id);

		/// <summary>
		/// Gets an entity with given primary key.
		/// </summary>
		/// <param name="id">Primary key of the entity to get</param>
		/// <returns>Entity</returns>
		Task<TAggregateRoot> GetAsync(Guid id);
	}
}