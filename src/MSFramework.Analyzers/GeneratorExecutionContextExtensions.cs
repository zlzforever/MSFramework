using Microsoft.CodeAnalysis;

/// <summary>
///
/// </summary>
// ReSharper disable once CheckNamespace
public static class GeneratorExecutionContextExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <param name="syntax"></param>
    /// <returns></returns>
    public static string GetFullName(this GeneratorExecutionContext context,
        SyntaxNode syntax)
    {
        var fullName = context.Compilation
            .GetSemanticModel(syntax.SyntaxTree)
            .GetDeclaredSymbol(syntax)?.ToDisplayString();
        return fullName;
    }

    private static readonly string AggregateRootInterface
        = "MicroserviceFramework.Domain.IAggregateRoot<";

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <param name="name"></param>
    public static (ITypeSymbol Type, bool IsAggregateRoot, string Key) GetAggregateRootInfo(
        this GeneratorExecutionContext context,
        string name)
    {
        var typeSymbol = context.Compilation
            .GetTypeByMetadataName(name);
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
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

        // var fullNamePieces = fullName.Split('.').ToList();
        // var split = "AggregateRoots";
        // var interval = fullNamePieces.IndexOf(split);
        // var pre = string.Join(".", fullNamePieces.Take(interval)) + ".Interface";
        // var takeLast = fullNamePieces.Count - interval - 1;
        // var sufItems =
        //     fullNamePieces.GetRange(interval + 1, fullNamePieces.Count - 1);
        // var suf = takeLast > 1 ? "." + string.Join(".", sufItems) : "";
        // var rootNamespaceName = $"{pre}{suf}";
        // return rootNamespaceName;
    }
}
