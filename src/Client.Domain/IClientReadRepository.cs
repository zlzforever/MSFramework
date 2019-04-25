using MSFramework.DependencyInjection;
using MSFramework.Domain.Repository;
using Client.Domain.AggregateRoot;

namespace Client.Domain
{
	public interface IClientReadRepository : IReadRepository<AggregateRoot.Client>, IScopeDependency
	{
	}


	public interface IClientWriteRepository : IWriteRepository<AggregateRoot.Client>, IScopeDependency
	{
	}
}