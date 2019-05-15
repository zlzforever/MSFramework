using System;
using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IMSFrameworkSession
	{
		string UserId { get; }

		string UserName { get; }

		Task CommitAsync();

		Task TrackAsync<TAggregateRoot>(TAggregateRoot aggregateRoot) where TAggregateRoot : AggregateRootBase;

		Task<TAggregateRoot> GetAsync<TAggregateRoot>(Guid aggregateRootId, int? expectedVersion = null)
			where TAggregateRoot : AggregateRootBase;

		Task<string> GetTokenAsync(string tokenName = "access_token");
	}
}