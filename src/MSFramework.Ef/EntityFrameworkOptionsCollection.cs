using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MSFramework.Ef
{
	public class EntityFrameworkOptionsCollection
	{
		private readonly Dictionary<string, EntityFrameworkOptions> _dict;
		private readonly Dictionary<Type, EntityFrameworkOptions> _contextTypeMapOptionsDict;

		private EntityFrameworkOptionsCollection(Dictionary<string, EntityFrameworkOptions> dict)
		{
			if (dict == null || dict.Count == 0)
			{
				throw new MSFrameworkException("未能找到数据上下文配置");
			}

			var repeated = dict.Values.GroupBy(m => m.DbContextType)
				.FirstOrDefault(m => m.Count() > 1);
			if (repeated != null)
			{
				throw new MSFrameworkException($"数据上下文配置中存在多个配置节点指向同一个上下文类型：{repeated.First().DbContextTypeName}");
			}

			_dict = dict;
			_contextTypeMapOptionsDict = new Dictionary<Type, EntityFrameworkOptions>();
			foreach (var value in dict.Values)
			{
				_contextTypeMapOptionsDict.Add(value.DbContextType, value);
			}
		}

		public IEnumerable<EntityFrameworkOptions> GetAllOptions()
		{
			return _dict.Values;
		}

		public EntityFrameworkOptions Get(Type contextType)
		{
			_contextTypeMapOptionsDict.TryGetValue(contextType, out var options);
			return options;
		}

		public static EntityFrameworkOptionsCollection LoadFrom(IConfiguration configuration)
		{
			var section = configuration.GetSection("DbContexts");
			return new EntityFrameworkOptionsCollection(section.Get<Dictionary<string, EntityFrameworkOptions>>());
		}
	}
}