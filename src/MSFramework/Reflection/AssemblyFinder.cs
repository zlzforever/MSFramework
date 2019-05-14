using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace MSFramework.Reflection
{
	/// <summary>
	/// 应用程序目录程序集查找器, TODO: 如何设计自动缓存, 自动更新, 查找以查现高性能
	/// </summary>
	public class AssemblyFinder : IAssemblyFinder
	{
		public List<Assembly> GetAllAssemblyList()
		{
			DependencyContext context = DependencyContext.Default;
			var assemblyNames = context.RuntimeLibraries.Where(x => x.Type == "project").Select(x => x.Name)
				.ToList();
			// 应用程序域中的程序集是动态变化的
			var assemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => assemblyNames.Contains(x.GetName().Name)).ToList();

			return assemblies;
		}
	}
}