using System;
using System.Collections.Generic;
using MicroserviceFramework.Configuration;

namespace MicroserviceFramework.Ef
{
	[OptionsType("DbContexts")]
	public class EntityFrameworkOptionsDictionary : Dictionary<string, EntityFrameworkOptions>
	{
		private Dictionary<Type, EntityFrameworkOptions> _dict;

		public IEnumerable<EntityFrameworkOptions> GetAllOptions()
		{
			return Values;
		}

		public EntityFrameworkOptions Get(Type contextType)
		{
			if (_dict == null)
			{
				// 若是 Singleton 则已经在 Initializer 里初始化了
				// 若是 Scoped、Transient 则不会有线程安全问题?
				_dict = new Dictionary<Type, EntityFrameworkOptions>();
				foreach (var value in Values)
				{
					_dict.Add(value.DbContextType, value);
				}
			}

			_dict.TryGetValue(contextType, out var options);
			return options;
		}
	}
}