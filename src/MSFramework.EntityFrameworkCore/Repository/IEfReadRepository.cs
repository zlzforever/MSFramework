using MSFramework.Domain;
using MSFramework.Domain.Repository;

namespace MSFramework.EntityFrameworkCore.Repository
{
	public interface IEfReadRepository<TAggregateRoot> : IReadRepository<TAggregateRoot>
		where TAggregateRoot : AggregateRootBase
	{
	}
}