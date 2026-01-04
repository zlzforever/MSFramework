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
    private static readonly List<Assembly> Assemblies = new();
    private static readonly List<Type> Types = new();
    private static readonly object Locker = new object();

    /// <summary>
    /// 请在 AddMicroserviceFramework 前添加前缀
    /// </summary>
    public static readonly HashSet<string> StartsWith = ["MSFramework"];

    /// <summary>
    ///
    /// </summary>
    public static readonly HashSet<string> ExcludeWith = new();

    internal static void Load()
    {
        lock (Locker)
        {
            Assemblies.Clear();
            Types.Clear();

            // 分析器不会输出程序集文件
            var analyzerAssemblyList = new[] { "MSFramework.Analyzers", "MSFramework.Ef.Analyzers" };
            if (DependencyContext.Default != null)
            {
                var dict = new Dictionary<string, Assembly>();
                var loadedAssemblies = new Dictionary<string, Assembly>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var name = assembly.GetName().Name;
                    loadedAssemblies.TryAdd(name, assembly);
                }

                var libraries = DependencyContext.Default.CompileLibraries
                    .Where(x => x.Type == "project"
                                || StartsWith.Any(y => x.Name.StartsWith(y)));
                foreach (var lib in libraries)
                {
                    if (lib.Type == "reference" || analyzerAssemblyList.Contains(lib.Name))
                    {
                        continue;
                    }

                    if (ExcludeWith.Any(y => lib.Name.StartsWith(y)))
                    {
                        continue;
                    }

                    if (!loadedAssemblies.TryGetValue(lib.Name, out var assembly))
                    {
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{lib.Name}.dll");
                        if (File.Exists(path))
                        {
                            assembly = AppDomain.CurrentDomain.Load(new AssemblyName(lib.Name));
                            loadedAssemblies.TryAdd(lib.Name, assembly);
                        }
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

                    if (ExcludeWith.Any(y => name.StartsWith(y)))
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

                Assemblies.AddRange(dict.Values.Where(x => x != null));
            }

            foreach (var assembly in Assemblies)
            {
                foreach (var definedType in assembly.DefinedTypes)
                {
                    Types.Add(definedType.AsType());
                }
            }
        }
    }

    /// <summary>
    /// 获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyCollection<Assembly> GetAllAssemblies()
    {
        lock (Locker)
        {
            return Assemblies.AsReadOnly();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyCollection<Type> GetAllTypes()
    {
        lock (Locker)
        {
            return Types.AsReadOnly();
        }
    }
}
