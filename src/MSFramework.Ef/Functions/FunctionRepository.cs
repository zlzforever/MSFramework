using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Ef.Infrastructure;
using MicroserviceFramework.Functions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MicroserviceFramework.Ef.Functions
{
	public class FunctionRepository : IFunctionRepository
	{
		private readonly IMemoryCache _cache;
		private readonly TimeSpan _ttl = new TimeSpan(0, 5, 0);
		private readonly IQueryable<Function> _currentSet;
		private readonly DbContext _dbContext;
		private readonly IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;

		public FunctionRepository(DbContextFactory dbContextFactory,
			IMemoryCache cache,
			IEntityConfigurationTypeFinder entityConfigurationTypeFinder)
		{
			_cache = cache;
			_entityConfigurationTypeFinder = entityConfigurationTypeFinder;

			if (!_entityConfigurationTypeFinder.HasDbContextForEntity<Function>())
			{
				return;
			}

			_dbContext = dbContextFactory.GetDbContext<Function>();
			_currentSet = _dbContext.Set<Function>();
		}

		public Function GetByCode(string code)
		{
			_cache.TryGetValue(code, out Function function);
			if (function == null)
			{
				function = _currentSet.AsNoTracking()
					.FirstOrDefault(x => x.Code == code);
				_cache.Set(code, function, _ttl);
			}

			return function;
		}

		public IEnumerable<Function> GetAllList()
		{
			return _currentSet;
		}

		public async Task InsertAsync(Function entity)
		{
			await _dbContext.Set<Function>().AddAsync(entity);
		}

		public Task UpdateAsync(Function entity)
		{
			var entry = _dbContext.Entry(entity);
			entry.State = EntityState.Modified;
			return Task.CompletedTask;
		}

		public bool IsAvailable()
		{
			return _currentSet != null;
		}
	}
}