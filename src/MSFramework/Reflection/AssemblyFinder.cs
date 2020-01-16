using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyModel;

namespace MSFramework.Reflection
{
	/// <summary>
	/// 应用程序目录程序集查找器
	/// </summary>
	public class AssemblyFinder : IAssemblyFinder
	{
		private List<Assembly> _assemblies;

		public static readonly HashSet<string> AssemblyNamePatterns = new HashSet<string>
		{
			"MSFramework"
		};

		public List<Assembly> GetAllAssemblyList()
		{
			if (_assemblies == null)
			{
				DependencyContext context = DependencyContext.Default;
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

				var assemblyNames = new List<string>();
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

				Console.WriteLine($"Find assemblies: {string.Join(", ", _assemblies.Select(x => x.GetName().Name))}");
			}

			return _assemblies;
		}
	}
}