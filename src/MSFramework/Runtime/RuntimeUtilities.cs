using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace MicroserviceFramework.Runtime;

public static class RuntimeUtilities
{
    private static readonly Lazy<Assembly[]> Assemblies;
    private static readonly Lazy<Type[]> Types;

    public static readonly List<string> StartsWith;

    static RuntimeUtilities()
    {
        StartsWith = new List<string>
        {
            "MSFramework",
            "Newtonsoft.Json"
        };
        Assemblies = new Lazy<Assembly[]>(() =>
        {
            var list = new List<Assembly>();
            var libraries = DependencyContext.Default.CompileLibraries
                .Where(x => x.Type == "project"
                            || StartsWith.Any(y => x.Name.StartsWith(y)));
            foreach (var lib in libraries)
            {
                var assembly = AppDomain.CurrentDomain.Load(new AssemblyName(lib.Name));
                list.Add(assembly);
            }

            return list.ToArray();
        });
        Types = new Lazy<Type[]>(() =>
            (from assembly in GetAllAssemblies() from type in assembly.DefinedTypes select type.AsType())
            .ToArray());
    }

    /// <summary>
    /// 获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包
    /// </summary>
    /// <returns></returns>
    public static Assembly[] GetAllAssemblies()
    {
        return Assemblies.Value;
    }

    public static IList<Type> GetAllTypes()
    {
        return Types.Value;
    }
}
