using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Ef.Infrastructure;
using MicroserviceFramework.Function;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MicroserviceFramework.Ef.Function
{
	public class FunctionRepository : IFunctionDefineRepository
	{
		private readonly IMemoryCache _cache;
		private readonly TimeSpan _ttl = new TimeSpan(0, 5, 0);
		private readonly IQueryable<FunctionDefine> _currentSet;
		private readonly DbContext _dbContext;

		public FunctionRepository(DbContextFactory dbContextFactory,
			IMemoryCache cache,
			IEntityConfigurationTypeFinder entityConfigurationTypeFinder)
		{
			_cache = cache;

			if (!entityConfigurationTypeFinder.HasDbContextForEntity<FunctionDefine>())
			{
				return;
			}

			_dbContext = dbContextFactory.GetDbContext<FunctionDefine>();
			_currentSet = _dbContext.Set<FunctionDefine>();
		}

		public FunctionDefine GetByCode(string code)
		{
			_cache.TryGetValue(code, out FunctionDefine function);
			if (function == null)
			{
				function = _currentSet
					.AsNoTracking()
					.FirstOrDefault(x => x.Code == code);
				_cache.Set(code, function, _ttl);
			}

			return function;
		}

		public IEnumerable<FunctionDefine> GetAllList()
		{
			return _currentSet;
		}

		public async Task InsertAsync(FunctionDefine entity)
		{
			await _dbContext.Set<FunctionDefine>().AddAsync(entity);
		}

		public Task UpdateAsync(FunctionDefine entity)
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