using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyModel;

namespace MSFramework.Reflection
{
	/// <summary>
	/// 应用程序目录程序集查找器，静态类，只搜索一次并缓存数据，不考虑动态类型等
	/// </summary>
	public class AssemblyFinder
	{
		private static List<Assembly> _assemblies;
		private static readonly object Locker = new object();

		public static readonly HashSet<string> AssemblyNamePatterns = new HashSet<string>
		{
			"MSFramework"
		};

		public static List<Assembly> GetAllList()
		{
			if (_assemblies == null)
			{
				lock (Locker)
				{
					if (_assemblies == null)
					{
						var context = DependencyContext.Default;
						var totalAssemblyNames = context.GetDefaultAssemblyNames().Select(x => x.Name).ToList();

						AssemblyNamePatterns.Add("MSFramework");
						AssemblyNamePatterns.Add(".+\\.Domain$");
						AssemblyNamePatterns.Add(".+\\.Infrastructure$");
						AssemblyNamePatterns.Add(".+\\.Application$");
						AssemblyNamePatterns.Add(".+\\.API$");

						var entryAssembly = Assembly.GetEntryAssembly();
						if (entryAssembly != null)
						{
							var entryAssemblyName = entryAssembly.GetName().Name;
							var startsWith = entryAssemblyName.Split('.').First();
							if (!AssemblyNamePatterns.Contains(startsWith))
							{
								AssemblyNamePatterns.Add(startsWith);
							}
						}

						var assemblyNames = new HashSet<string>();
						foreach (var assemblyName in totalAssemblyNames)
						{
							foreach (var pattern in AssemblyNamePatterns)
							{
								if (Regex.IsMatch(assemblyName, pattern))
								{
									assemblyNames.Add(assemblyName);
									break;
								}
							}
						}

						_assemblies = new List<Assembly>();

						var dict = new Dictionary<string, Assembly>();
						foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
						{
							var name = assembly.GetName().Name;
							if (!string.IsNullOrWhiteSpace(name) && !dict.ContainsKey(name))
							{
								dict.Add(name, assembly);
							}
						}

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
				}
			}

			return _assemblies;
		}
	}
}