using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MicroserviceFramework.Ef.Analyzers;

[Generator(LanguageNames.CSharp)]
public class RepositoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, static (context, compilation) =>
        {
            var log = new StringBuilder();
            log.AppendLine($"// <auto-generated time='{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}' />");

            if (compilation == null)
            {
                log.Append("// warning: ").AppendLine("Compilation is null");
                return;
            }

            var domainAssemblyName = compilation.AssemblyName?.Replace("Infrastructure", "Domain");
            if (string.IsNullOrEmpty(domainAssemblyName))
            {
                log.Append("// warning: ").AppendLine("AssemblyName is null");
                return;
            }

            var domainMetadataReference = compilation.References
                .FirstOrDefault(x => !string.IsNullOrEmpty(x.Display)
                                     && x.Display.Contains(domainAssemblyName));
            if (domainMetadataReference == null)
            {
                log.Append("// warning: ").AppendLine("Assembly {domainAssemblyName} is missing");
            }
            else
            {
                var assemblyOrModuleSymbol = compilation.GetAssemblyOrModuleSymbol(domainMetadataReference)
                    as IAssemblySymbol;

                var list = new HashSet<string>();
                if (assemblyOrModuleSymbol != null)
                {
                    list = LoadAllTypes(assemblyOrModuleSymbol.GlobalNamespace);
                }

                foreach (var entity in list)
                {
                    var aggregateRootInfo = compilation.GetAggregateRootInfo(entity);
                    if (!aggregateRootInfo.IsAggregateRoot)
                    {
                        // log.Append("// info: ignore class -> ").AppendLine(entityFullName);
                        continue;
                    }

                    // log.Append("// info: handle class -> ").AppendLine(entityFullName);

                    var typeInfo = aggregateRootInfo.Type.GetTypeInfo();
                    var namespaceName = typeInfo.Namespace.Replace("Domain", "Infrastructure");
                    var script = $$"""
                                   // <auto-generated time='{{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}}' />
                                   using MicroserviceFramework.Ef;
                                   using MicroserviceFramework.Ef.Repositories;
                                   using {{typeInfo.Namespace}};
                                   using E = {{entity}};

                                   namespace {{namespaceName}};

                                   public partial class {{typeInfo.TypeName}}Repository
                                       : EfRepository<E, {{aggregateRootInfo.Key}}>, I{{typeInfo.TypeName}}Repository
                                   {
                                       public {{typeInfo.TypeName}}Repository(DbContextFactory context) : base(context)
                                       {
                                           UseQuerySplittingBehavior = true;
                                       }
                                   }
                                   """;

                    context.AddSource($"{typeInfo.Namespace}.{typeInfo.TypeName}.g.cs",
                        SourceText.From(script, Encoding.UTF8));
                }
            }
        });
    }

    private static HashSet<string> LoadAllTypes(INamespaceSymbol namespaceSymbol)
    {
        var list = new HashSet<string>();
        foreach (var namespaceMember in namespaceSymbol
                     .GetNamespaceMembers())
        {
            var tmp = LoadAllTypes(namespaceMember);
            foreach (var item in tmp)
            {
                list.Add(item);
            }
        }

        foreach (var member in namespaceSymbol.GetMembers())
        {
            list.Add(member.ToDisplayString());
        }

        return list;
    }
}
