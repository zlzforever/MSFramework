using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace MSFramework.Reflection
{
	/// <summary>
	/// 应用程序目录程序集查找器
	/// </summary>
	public class AssemblyFinder : IAssemblyFinder
	{
		private List<Assembly> _assemblies;

		public static string[] StartsWithAssemblyNames = new[]
		{
			"MSFramework"
		};

		public List<Assembly> GetAllAssemblyList()
		{
			if (_assemblies == null)
			{
				DependencyContext context = DependencyContext.Default;
				var totalAssemblyNames = context.GetDefaultAssemblyNames().Select(x => x.Name).ToList();

				var startsWithAssemblyNameList = new HashSet<string>(StartsWithAssemblyNames);
				if (!startsWithAssemblyNameList.Contains("MSFramework"))
				{
					startsWithAssemblyNameList.Add("MSFramework");
				}

				var entryAssembly = Assembly.GetEntryAssembly();
				if (entryAssembly != null)
				{
					var entryAssemblyName = entryAssembly.GetName().Name;
					var startsWith = entryAssemblyName.Split('.').First();
					if (!startsWithAssemblyNameList.Contains(startsWith))
					{
						startsWithAssemblyNameList.Add(startsWith);
					}
				}

				var assemblyNames = new List<string>();
				foreach (var assemblyName in totalAssemblyNames)
				{
					foreach (var startsWithAssemblyName in startsWithAssemblyNameList)
					{
						if (assemblyName.StartsWith(startsWithAssemblyName))
						{
							assemblyNames.Add(assemblyName);
							break;
						}
					}
				}

				_assemblies = new List<Assembly>();
				var dict = AppDomain.CurrentDomain.GetAssemblies().ToDictionary(x => x.GetName().Name, x => x);
				foreach (var assemblyName in assemblyNames)
				{
					if (!dict.ContainsKey(assemblyName))
					{
						var assembly = AppDomain.CurrentDomain.Load(assemblyName);
						_assemblies.Add(assembly);
					}
					else
					{
						_assemblies.Add(dict[assemblyName]);
					}
				}
			}

			return _assemblies;
		}
	}
}