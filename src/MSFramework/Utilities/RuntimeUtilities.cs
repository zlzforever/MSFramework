using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace MicroserviceFramework.Utilities
{
	public static class RuntimeUtilities
	{
		private static readonly Lazy<List<Assembly>> Assemblies;
		private static readonly Lazy<List<Type>> Types;

		static RuntimeUtilities()
		{
			Assemblies = new Lazy<List<Assembly>>(() =>
			{
				var list = new List<Assembly>();
				var libraries = DependencyContext.Default.CompileLibraries
					.Where(x => x.Type == "project" || x.Name.StartsWith("MSFramework"));
				foreach (var lib in libraries)
				{
					try
					{
						var assembly = AppDomain.CurrentDomain.Load(new AssemblyName(lib.Name));
						list.Add(assembly);
					}
					catch (Exception)
					{
						// ignored
					}
				}

				return list;
			});
			Types = new Lazy<List<Type>>(() =>
				(from assembly in GetAllAssemblies() from type in assembly.DefinedTypes select type.AsType()).ToList());
		}

		/// <summary>
		/// 获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包
		/// </summary>
		/// <returns></returns>
		public static IList<Assembly> GetAllAssemblies()
		{
			return Assemblies.Value;
		}

		public static IList<Type> GetAllTypes()
		{
			return Types.Value;
		}

		public static Type GetImplementType(string typeName, Type baseInterfaceType)
		{
			return GetAllTypes().FirstOrDefault(t =>
			{
				if (t.Name == typeName &&
				    t.GetTypeInfo().GetInterfaces().Any(b => b.Name == baseInterfaceType.Name))
				{
					var typeInfo = t.GetTypeInfo();
					return typeInfo.IsClass && !typeInfo.IsAbstract && !typeInfo.IsGenericType;
				}

				return false;
			});
		}
	}
}