using System;
using System.Threading.Tasks;

namespace MSFramework.Domain.Repository
{
	public interface IAggregateRepository<TAggregate> : IRepository where TAggregate : IAggregateRoot
	{
		TAggregate Get(Guid id);

		Task<TAggregate> GetAsync(Guid id);

		TAggregate Insert(TAggregate aggregate);

		Task<TAggregate> InsertAsync(TAggregate aggregate);

		Task<TAggregate> UpdateAsync(TAggregate aggregate);

		Task DeleteAsync(TAggregate aggregate);

		Task DeleteAsync(Guid id);
	}
}
