using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Configuration;


namespace MSFramework.Ef
{
	public class EntityFrameworkOptionDict
	{
		public ConcurrentDictionary<string, EntityFrameworkOptions> Value { get; set; }

		private EntityFrameworkOptionDict()
		{
		}

		public static EntityFrameworkOptionDict LoadFrom(IConfiguration configuration)
		{
			var section = configuration.GetSection("DbContexts");
			var dict = new EntityFrameworkOptionDict
			{
				Value = section.Get<ConcurrentDictionary<string, EntityFrameworkOptions>>()
			};
			if (dict.Value == null ||
			    dict.Value.IsEmpty)
			{
				throw new MSFrameworkException("未能找到数据上下文配置");
			}

			var repeated = dict.Value.Values.GroupBy(m => m.DbContextType)
				.FirstOrDefault(m => m.Count() > 1);
			if (repeated != null)
			{
				throw new MSFrameworkException($"数据上下文配置中存在多个配置节点指向同一个上下文类型：{repeated.First().DbContextTypeName}");
			}

			return dict;
		}
	}
}