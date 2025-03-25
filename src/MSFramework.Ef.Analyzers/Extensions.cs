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
        var tmp = fullName.Replace("AggregateRoots", "Repositories")
            .Replace("Aggregates", "Repositories");
        var index = tmp.LastIndexOf('.');
        var final = tmp.Substring(0, index);
        return final;
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
        }

        return (typeSymbol, isAggregateRoot, key);
    }
}
