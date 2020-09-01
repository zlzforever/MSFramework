using System;
using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.DependencyInjection
{
	/// <summary>
	/// <see cref="ServiceLifetime.Scoped"/>生命周期类型的服务映射查找器
	/// </summary>
	public class DependencyTypeFinder
	{
		private static Dictionary<ServiceLifetime, Type[]> _dict;
		private static readonly object Locker = new object();

		public static Dictionary<ServiceLifetime, Type[]> GetDependencyTypeDict()
		{
			if (_dict == null)
			{
				lock (Locker)
				{
					if (_dict == null)
					{
						var scope = typeof(IScopeDependency);
						var singleton = typeof(ISingletonDependency);
						var transient = typeof(ITransientDependency);

						var assemblies = AssemblyFinder.GetAllList();
						var scopeTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
							.Where(type => scope.IsAssignableFrom(type) && !type.IsAbstract &&
							               !type.IsInterface).ToArray();
						var singletonTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
							.Where(type => singleton.IsAssignableFrom(type) && !type.IsAbstract &&
							               !type.IsInterface).ToArray();
						var transientTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
							.Where(type => transient.IsAssignableFrom(type) && !type.IsAbstract &&
							               !type.IsInterface).ToArray();
						_dict = new Dictionary<ServiceLifetime, Type[]>
						{
							{ServiceLifetime.Scoped, scopeTypes},
							{ServiceLifetime.Singleton, singletonTypes},
							{ServiceLifetime.Transient, transientTypes}
						};
					}
				}
			}

			return _dict;
		}
	}
}