using Microsoft.CodeAnalysis;

namespace MicroserviceFramework.Ef.Analyzers;

public static class Extensions
{
    private static readonly string AggregateRootInterface
        = "MicroserviceFramework.Domain.IAggregateRoot<";

    public static (string Namespace, string TypeName) GetTypeInfo(
        this ITypeSymbol typeSymbol)
    {
        var name = typeSymbol.ToDisplayString(new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        var rootNamespaceName = GetNamespace(name);
        var aggregateRoot = typeSymbol.Name;
        return (rootNamespaceName, aggregateRoot);
    }

    public static string GetNamespace(string fullName)
    {
        // Ordering.Domain.AggregateRoots.Product -> Ordering.Domain.Interface
        // Ordering.Domain.AggregateRoots.Product -> Ordering.Infrastructure.Interface
        var tmp = ReplaceNamespaceSegment(fullName, "AggregateRoots", "Repositories");
        tmp = ReplaceNamespaceSegment(tmp, "Aggregates", "Repositories");
        // 全局命名空间（无 '.' 分隔符）时直接返回，避免 LastIndexOf 为 -1 时 Substring 崩溃
        var index = tmp.LastIndexOf('.');
        if (index <= 0)
        {
            return tmp;
        }

        var final = tmp.Substring(0, index);
        return final;
    }

    /// <summary>
    /// 按命名空间段精确替换，避免 Replace 全量替换误伤其他包含该子串的段
    /// </summary>
    /// <param name="fullName">完整命名空间</param>
    /// <param name="segment">待替换的命名空间段</param>
    /// <param name="replacement">替换后的命名空间段</param>
    /// <returns>替换后的命名空间</returns>
    public static string ReplaceNamespaceSegment(string fullName, string segment, string replacement)
    {
        var parts = fullName.Split('.');
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i] == segment)
            {
                parts[i] = replacement;
            }
        }

        return string.Join(".", parts);
    }

    public static (ITypeSymbol Type, bool IsAggregateRoot, string Key) GetAggregateRootInfo(
        this Compilation compilation,
        string name)
    {
        var typeSymbol = compilation.GetTypeByMetadataName(name);
        if (typeSymbol == null)
        {
            return (null, false, null);
        }

        // 获取所有继承的接口
        var interfaces = typeSymbol.AllInterfaces.ToList();
        var isAggregateRoot = false;
        string key = null;
        if (interfaces is { Count: > 0 })
        {
            foreach (var @interface in interfaces)
            {
                var interfaceName = @interface.ToDisplayString();
                if (!interfaceName.StartsWith(AggregateRootInterface))
                {
                    continue;
                }

                isAggregateRoot = true;
                key = interfaceName.Substring(
                    AggregateRootInterface.Length,
                    interfaceName.Length - AggregateRootInterface.Length - 1);
                break;
            }

            // 泛型接口未命中时，检查是否实现非泛型 IAggregateRoot（复合主键多属性实体，无键）
            if (!isAggregateRoot && interfaces.Any(x =>
                    x.ToDisplayString() == "MicroserviceFramework.Domain.IAggregateRoot"))
            {
                isAggregateRoot = true;
            }
        }

        return (typeSymbol, isAggregateRoot, key);
    }
}
