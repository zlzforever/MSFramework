using System;
using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.Utilities;

namespace MicroserviceFramework.Ef.Infrastructure
{
	/// <summary>
	/// 实体类配置类型查找器
	/// </summary>
	public class EntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
	{
		private readonly IDictionary<Type, IEntityRegister[]> _entityRegistersDict
			= new Dictionary<Type, IEntityRegister[]>();

		private readonly IDictionary<Type, Type> _entityMapDbContextDict = new Dictionary<Type, Type>();

		/// <summary>
		/// 初始化
		/// </summary>
		void IEntityConfigurationTypeFinder.Initialize()
		{
			var dict = _entityRegistersDict;
			dict.Clear();

			var assemblies = RuntimeUtilities.GetAllAssemblies();
			var types = assemblies.SelectMany(assembly => assembly.GetTypes()).Where(type =>
					type.IsClass && !type.IsAbstract && typeof(IEntityRegister).IsAssignableFrom(type))
				.Distinct()
				.ToList();
			if (types.Count == 0)
			{
				return;
			}

			var registers =
				types.Select(type => Activator.CreateInstance(type) as IEntityRegister).ToList();
			var groups = registers.GroupBy(m => m.DbContextType).ToList();
			Type key;
			foreach (var group in groups)
			{
				key = group.Key ?? typeof(DefaultDbContext);
				var list = new List<IEntityRegister>();
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
				var list = dict[key].ToList();

				//TODO: list.Add(new AuditOperationConfiguration());
				dict[key] = list.ToArray();
			}

			foreach (var register in registers)
			{
				if (_entityMapDbContextDict.ContainsKey(register.EntityType))
				{
					continue;
				}

				_entityMapDbContextDict.Add(register.EntityType, register.DbContextType);
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
			if (!_entityMapDbContextDict.ContainsKey(entityType))
			{
				throw new MicroserviceFrameworkException(
					"未发现任何数据上下文实体映射配置， 请通过对各个实体继承基类“EntityTypeConfigurationBase<TEntity, TKey>”以使实体加载到上下文中");
			}

			return _entityMapDbContextDict[entityType];
		}

		public bool HasDbContextForEntity<T>()
		{
			return _entityMapDbContextDict.ContainsKey(typeof(T));
		}
	}
}