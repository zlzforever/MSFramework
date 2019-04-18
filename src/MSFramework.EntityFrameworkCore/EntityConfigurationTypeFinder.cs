using System;
using System.Collections.Generic;
using System.Linq;
using MSFramework.Core;
using MSFramework.Reflection;

namespace MSFramework.EntityFrameworkCore
{
	/// <summary>
	/// 实体类配置类型查找器
	/// </summary>
	public class EntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
	{
		private readonly IDictionary<Type, IEntityRegister[]> _entityRegistersDict
			= new Dictionary<Type, IEntityRegister[]>();

		/// <summary>
		/// 初始化
		/// </summary>
		public void Initialize()
		{
			IDictionary<Type, IEntityRegister[]> dict = _entityRegistersDict;
			dict.Clear();
			var assemblyFinder = Singleton<IAssemblyFinder>.Instance ?? new AssemblyFinder();
			var assemblies = assemblyFinder.GetAllAssemblyList();
			var types = assemblies.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.IsClass && !type.IsAbstract && type.IsAssignableFrom(typeof(IEntityRegister)))
				.Distinct()
				.ToArray();
			if (types.Length == 0)
			{
				return;
			}

			List<IEntityRegister> registers =
				types.Select(type => Activator.CreateInstance(type) as IEntityRegister).ToList();
			List<IGrouping<Type, IEntityRegister>> groups = registers.GroupBy(m => m.DbContextType).ToList();
			Type key;
			foreach (IGrouping<Type, IEntityRegister> group in groups)
			{
				key = group.Key ?? typeof(DefaultDbContext);
				List<IEntityRegister> list = new List<IEntityRegister>();
				if (group.Key == null || group.Key == typeof(DefaultDbContext))
				{
					list.AddRange(group);
				}
				else
				{
					list = group.ToList();
				}

				if (list.Count > 0)
				{
					dict[key] = list.ToArray();
				}
			}

			//添加框架的一些默认实体的实体映射信息（如果不存在）
			key = typeof(DefaultDbContext);
			if (dict.ContainsKey(key))
			{
				List<IEntityRegister> list = dict[key].ToList();

				//TODO: list.Add(new AuditOperationConfiguration());
				dict[key] = list.ToArray();
			}
		}

		/// <summary>
		/// 获取指定上下文类型的实体配置注册信息
		/// </summary>
		/// <param name="dbContextType">数据上下文类型</param>
		/// <returns></returns>
		public IEntityRegister[] GetEntityRegisters(Type dbContextType)
		{
			return _entityRegistersDict.ContainsKey(dbContextType)
				? _entityRegistersDict[dbContextType]
				: new IEntityRegister[0];
		}

		/// <summary>
		/// 获取 实体类所属的数据上下文类
		/// </summary>
		/// <param name="entityType">实体类型</param>
		/// <returns>数据上下文类型</returns>
		public Type GetDbContextTypeForEntity(Type entityType)
		{
			var dict = _entityRegistersDict;
			if (dict.Count == 0)
			{
				throw new MSFrameworkException(
					"未发现任何数据上下文实体映射配置，请通过对各个实体继承基类“EntityTypeConfigurationBase<TEntity, TKey>”以使实体加载到上下文中");
			}

			foreach (var item in _entityRegistersDict)
			{
				if (item.Value.Any(m => m.EntityType == entityType))
				{
					return item.Key;
				}
			}

			throw new MSFrameworkException(
				$"无法获取实体类“{entityType}”的所属上下文类型，请通过继承基类“EntityTypeConfigurationBase<TEntity, TKey>”配置实体加载到上下文中");
		}
	}
}