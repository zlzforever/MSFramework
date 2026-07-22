using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyModel;

namespace MicroserviceFramework.Utils;

/// <summary>
/// 只管理业务程序集
/// </summary>
public static class Runtime
{
    private static FrozenSet<Assembly> _assemblies;
    private static FrozenSet<Type> _types;
    private static readonly Lock Locker = new();

    /// <summary>
    /// 请在 AddMicroserviceFramework 前添加前缀
    /// </summary>
    public static readonly HashSet<string> StartsWith = ["MSFramework"];

    /// <summary>
    /// 排除的程序集前缀集合，这些程序集不会被扫描加载
    /// </summary>
    public static readonly HashSet<string> ExcludeWith = new();

    /// <summary>
    /// 加载所有符合前缀匹配的业务程序集，初始化程序集和类型缓存。
    /// </summary>
    internal static void Load()
    {
        if (_assemblies != null)
        {
            return;
        }

        lock (Locker)
        {
            var assemblies = new List<Assembly>();
            var types = new HashSet<Type>();
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

                // var libraries = DependencyContext.Default.CompileLibraries
                //     .Where(x => x.Type == "project"
                //                 || StartsWith.Any(y => x.Name.StartsWith(y)));

                // 只加载业务 Assembly
                var libraries = DependencyContext.Default.CompileLibraries
                    .Where(x => StartsWith.Any(y => x.Name.StartsWith(y)));

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

                    if (!StartsWith.Any(name.StartsWith))
                    {
                        continue;
                    }

                    if (ExcludeWith.Any(name.StartsWith))
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

                assemblies.AddRange(dict.Values.Where(x => x != null));
            }

            _assemblies = assemblies.ToFrozenSet();

            foreach (var assembly in _assemblies)
            {
                foreach (var definedType in assembly.DefinedTypes)
                {
                    types.Add(definedType.AsType());
                }
            }

            _types = types.ToFrozenSet();
        }
    }

    /// <summary>
    /// 获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包
    /// </summary>
    /// <returns></returns>
    public static FrozenSet<Assembly> GetAllAssemblies()
    {
        return _assemblies;
    }

    /// <summary>
    /// 获取所有已加载的程序集类型
    /// </summary>
    /// <returns>所有已加载的程序集类型集合</returns>
    public static FrozenSet<Type> GetAllTypes()
    {
        return _types;
    }
}
