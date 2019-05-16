using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.Domain.Exception;
using MSFramework.Domain.Repository;

namespace MSFramework.Domain
{
	public abstract class MSFrameworkSessionBase : IMSFrameworkSession
	{
		private readonly IRepository _repository;
		private readonly Dictionary<Guid, AggregateRootBase> _trackedAggregateRoots;

		protected MSFrameworkSessionBase(IRepository repository)
		{
			_repository = repository;
			_trackedAggregateRoots = new Dictionary<Guid, AggregateRootBase>();
		}

		public abstract string UserId { get; }

		public abstract string UserName { get; }

		public virtual async Task CommitAsync()
		{
			await _repository.AppendAsync(_trackedAggregateRoots.Values);
			_trackedAggregateRoots.Clear();
		}

		public virtual Task AddAsync<TAggregateRoot>(TAggregateRoot aggregateRoot)
			where TAggregateRoot : AggregateRootBase
		{
			if (!_trackedAggregateRoots.ContainsKey(aggregateRoot.Id))
			{
				_trackedAggregateRoots.Add(aggregateRoot.Id, aggregateRoot);
			}
			else if (_trackedAggregateRoots[aggregateRoot.Id] != aggregateRoot)
			{
				throw new ConcurrencyException(aggregateRoot.Id);
			}

			return Task.CompletedTask;
		}

		public virtual async Task<TAggregateRoot> GetAsync<TAggregateRoot>(Guid aggregateRootId,
			int? expectedVersion = null)
			where TAggregateRoot : AggregateRootBase
		{
			if (_trackedAggregateRoots.ContainsKey(aggregateRootId))
			{
				var trackedAggregateRoot = (TAggregateRoot) _trackedAggregateRoots[aggregateRootId];
				if (expectedVersion != null && trackedAggregateRoot.Version != expectedVersion)
				{
					throw new ConcurrencyException(trackedAggregateRoot.Id);
				}

				return trackedAggregateRoot;
			}

			var aggregateRoot = await _repository.GetAsync<TAggregateRoot>(aggregateRootId);
			if (expectedVersion != null && aggregateRoot.Version != expectedVersion)
			{
				throw new ConcurrencyException(aggregateRootId);
			}

			await AddAsync(aggregateRoot);

			return aggregateRoot;
		}

		public abstract Task<string> GetTokenAsync(string tokenName = "access_token");
	}
}