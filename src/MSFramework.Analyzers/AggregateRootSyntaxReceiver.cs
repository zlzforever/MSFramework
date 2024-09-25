using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
///
/// </summary>
// ReSharper disable once CheckNamespace
internal class AggregateRootSyntaxReceiver : ISyntaxReceiver
{
    private static readonly List<string> AggregateRootBaseTypeList =
    [
        "CreationAggregateRoot", "DeletionAggregateRoot",
        "ModificationAggregateRoot", "IAggregateRoot"
    ];

    /// <summary>
    ///
    /// </summary>
    public readonly Dictionary<ClassDeclarationSyntax, BaseTypeSyntax> ClassDeclarationList = [];

    /// <summary>
    ///
    /// </summary>
    /// <param name="syntaxNode"></param>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax { BaseList: not null } classDeclaration)
        {
            // CreationAggregateRoot CreationAggregateRoot<>
            // DeletionAggregateRoot DeletionAggregateRoot<>
            // ModificationAggregateRoot ModificationAggregateRoot<>
            // IAggregateRoot IAggregateRoot<>
            var aggregateRootBase = classDeclaration
                .BaseList.Types
                .FirstOrDefault(x =>
                    AggregateRootBaseTypeList.Any(y => x.ToString().Contains(y)));

            if (aggregateRootBase != null)
            {
                ClassDeclarationList.Add(classDeclaration, aggregateRootBase);
            }
        }
    }
}
