using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.FeatureManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MicroserviceFramework.Ef.FeatureManagement
{
	public class FeatureRepository : IFeatureRepository
	{
		private readonly IMemoryCache _cache;
		private readonly TimeSpan _ttl = new(0, 5, 0);
		private readonly DbContext _dbContext;
		private readonly bool _isAvailable;

		public FeatureRepository(DbContextFactory dbContextFactory,
			IMemoryCache cache)
		{
			_cache = cache;
			_dbContext = dbContextFactory.GetDbContextOrDefault<Feature>();
			_isAvailable = _dbContext != null;
		}

		public Feature GetByName(string name)
		{
			_cache.TryGetValue(name, out Feature function);
			if (function == null)
			{
				function = _dbContext.Set<Feature>()
					.AsNoTracking()
					.FirstOrDefault(x => x.Name == name);
				_cache.Set(name, function, _ttl);
			}

			return function;
		}

		public IEnumerable<Feature> GetAllList()
		{
			return _dbContext.Set<Feature>().AsNoTracking();
		}

		public async Task AddAsync(Feature entity)
		{
			await _dbContext.Set<Feature>().AddAsync(entity);
		}

		public bool IsAvailable()
		{
			return _isAvailable;
		}
	}
}