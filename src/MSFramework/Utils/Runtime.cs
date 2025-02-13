using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace MicroserviceFramework.Utils;

/// <summary>
///
/// </summary>
public static class Runtime
{
    private static readonly Lazy<List<Assembly>> Assemblies;
    private static readonly Lazy<List<Type>> Types;
    internal static readonly HashSet<string> StartsWith;

    static Runtime()
    {
        // StartsWith = ["MSFramework", "Newtonsoft.Json"];
        StartsWith = ["MSFramework"];
        // 分析器不会输出程序集文件
        var analyzerAssemblyList = new[] { "MSFramework.Analyzers", "MSFramework.Ef.Analyzers" };
        Assemblies = new Lazy<List<Assembly>>(() =>
        {
            if (DependencyContext.Default == null)
            {
                return [];
            }

            var dict = new Dictionary<string, Assembly>();
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .ToDictionary(x => x.FullName, x => x);
            var libraries = DependencyContext.Default.CompileLibraries
                .Where(x => x.Type == "project"
                            || StartsWith.Any(y => x.Name.StartsWith(y)));
            foreach (var lib in libraries)
            {
                if (lib.Type == "reference" || analyzerAssemblyList.Contains(lib.Name))
                {
                    continue;
                }

                if (!loadedAssemblies.TryGetValue(lib.Name, out var assembly))
                {
                    assembly = AppDomain.CurrentDomain.Load(new AssemblyName(lib.Name));
                    loadedAssemblies.Add(lib.Name, assembly);
                }

                dict.TryAdd(lib.Name, assembly);
            }

            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll",
                SearchOption.TopDirectoryOnly).ToList();
            var pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            if (Directory.Exists(pluginsPath))
            {
                files.AddRange(Directory.GetFiles(pluginsPath, "*.dll",
                    SearchOption.TopDirectoryOnly));
            }

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (analyzerAssemblyList.Contains(name))
                {
                    continue;
                }

                if (!StartsWith.Any(y => name.StartsWith(y)))
                {
                    continue;
                }

                if (dict.ContainsKey(name))
                {
                    continue;
                }

                var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(file));
                dict.TryAdd(name, assembly);
            }

            return dict.Values.ToList();
        });
        Types = new Lazy<List<Type>>(() =>
            (from assembly in GetAllAssemblies() from type in assembly.DefinedTypes select type.AsType()).ToList());
    }

    /// <summary>
    /// 获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyCollection<Assembly> GetAllAssemblies()
    {
        return Assemblies.Value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyCollection<Type> GetAllTypes()
    {
        return Types.Value;
    }
}
