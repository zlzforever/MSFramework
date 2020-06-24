using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using MSFramework.Function;

namespace MSFramework.Ef.Function
{
	public class EfFunctionStore : IFunctionStore
	{
		private readonly EfRepository<FunctionDefine> _repository;
		private readonly IMemoryCache _cache;
		private readonly TimeSpan _ttl = new TimeSpan(0, 1, 0);

		public EfFunctionStore(IMemoryCache cache, EfRepository<FunctionDefine> efRepository)
		{
			_repository = efRepository;
			_cache = cache;
		}

		public FunctionDefine Get(string path)
		{
			_cache.TryGetValue(path, out FunctionDefine function);
			if (function == null)
			{
				function = _repository.CurrentSet.FirstOrDefault(x => x.Path == path);
				_cache.Set(path, function, _ttl);
			}

			return function;
		}

		public List<FunctionDefine> GetAllList()
		{
			return _repository.CurrentSet.ToList();
		}

		public void Update(FunctionDefine function)
		{
			_repository.Update(function);
			_cache.Set(function.Path, function, _ttl);
		}

		public void Delete(Guid id)
		{
			var entity = _repository.Delete(id);
			if (entity != null)
			{
				_cache.Remove(entity.Path);
			}
		}

		public void Add(FunctionDefine function)
		{
			_repository.Insert(function);
			_cache.Set(function.Path, function, _ttl);
		}
	}
}