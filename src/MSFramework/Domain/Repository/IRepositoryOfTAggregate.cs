using System;
using System.Threading.Tasks;

namespace MSFramework.Domain.Repository
{
	public interface IAggregateRepository<TAggregate, in TPrimaryKey> : IRepository where TAggregate : IAggregateRoot<TPrimaryKey>
		where TPrimaryKey :IEquatable<TPrimaryKey>
	{
		TAggregate Get(TPrimaryKey id);

		Task<TAggregate> GetAsync(TPrimaryKey id);

		TAggregate Insert(TAggregate aggregate);

		Task<TAggregate> InsertAsync(TAggregate aggregate);

		Task<TAggregate> UpdateAsync(TAggregate aggregate);

		Task DeleteAsync(TAggregate aggregate);

		Task DeleteAsync(TPrimaryKey id);
	}
}
