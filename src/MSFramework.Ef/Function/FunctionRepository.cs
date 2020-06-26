using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MSFramework.Ef.Repository;
using MSFramework.Function;

namespace MSFramework.Ef.Function
{
	public class FunctionRepository : EfRepository<FunctionDefine, Guid>, IFunctionRepository
	{
		private readonly IMemoryCache _cache;
		private readonly TimeSpan _ttl = new TimeSpan(0, 5, 0);

		public FunctionRepository(DbContextFactory dbContextFactory, IMemoryCache cache) : base(dbContextFactory)
		{
			_cache = cache;
		}

		public FunctionDefine GetByCode(string code)
		{
			_cache.TryGetValue(code, out FunctionDefine function);
			if (function == null)
			{
				function = CurrentSet.AsNoTracking()
					.FirstOrDefault(x => x.Code == code);
				_cache.Set(code, function, _ttl);
			}

			return function;
		}

		public IEnumerable<FunctionDefine> GetAllList()
		{
			return CurrentSet;
		}
	}
}