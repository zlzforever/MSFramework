using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MicroserviceFramework.Analyzers;

/// <summary>
/// 仓储<b>接口</b>源生成器（Domain 层）：
/// 扫描实现了 <see cref="IAggregateRoot{TKey}"/>（或非泛型 <see cref="IAggregateRoot"/>）的公共类，
/// 按命名空间段约定在 Repositories 段生成 <c>I{X}Repository : IRepository&lt;E, TKey&gt;, IScopeDependency</c>
/// 部分接口，供领域层直接依赖。
/// <para>
/// 依赖 MSFramework.Ef.Analyzers 的 <c>RepositoryGenerator</c>（Infrastructure 层）生成对应实现，
/// 两者的命名空间段映射约定（AggregateRoots/Aggregates → Repositories、Domain → Infrastructure）
/// 必须保持一致，否则接口与实现会落位失配。
/// 约定段校验见 <see cref="MissingAggregateSegmentRule"/>（MSF0002），全局命名空间见
/// <see cref="GlobalNamespaceAggregateRootRule"/>（MSF0001）
/// </para>
/// </summary>
[Generator(LanguageNames.CSharp)]
public class RepositoryInterfaceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// 全局命名空间聚合根的诊断规则：无法生成仓储接口/实现，以编译错误显式上报
    /// </summary>
    private static readonly DiagnosticDescriptor GlobalNamespaceAggregateRootRule = new(
        "MSF0001",
        "全局命名空间的聚合根无法生成仓储接口",
        "聚合根 '{0}' 位于全局命名空间（无 namespace），无法生成仓储接口：" +
        "请为类型添加显式命名空间，且命名空间需包含 'AggregateRoots'（或 'Aggregates'）段",
        "MSFramework.SourceGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "仓储接口与实现由源生成器按命名空间段（AggregateRoots → Repositories）推导生成，" +
        "聚合根必须位于显式命名空间中；全局命名空间的类型会被跳过，这里显式报错以便定位.");

    /// <summary>
    /// 聚合根未放在约定命名空间段的诊断规则：跳过生成，以警告显式上报。
    /// 与文档约定一致（AGENTS.md：仅当聚合根命名空间包含 AggregateRoots/Aggregates 段才生成），
    /// 避免在任意命名空间下静默生成仓储接口造成误生成/命名冲突，
    /// 也避免与实现生成器（按段映射 Domain→Infrastructure）的行为不一致
    /// </summary>
    private static readonly DiagnosticDescriptor MissingAggregateSegmentRule = new(
        "MSF0002",
        "聚合根未放在约定的命名空间段下，未生成仓储接口",
        "类型 '{0}' 实现了 IAggregateRoot 但命名空间 '{1}' 不包含 'AggregateRoots'（或 'Aggregates'）段，" +
        "未生成仓储接口：请将聚合根移入 *.AggregateRoots（或 *.Aggregates）命名空间段",
        "MSFramework.SourceGeneration",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "仓储接口/实现的生成依赖命名空间段约定（AggregateRoots/Aggregates → Repositories、" +
        "Domain → Infrastructure）；不在约定段下的聚合根会被跳过，避免在任意命名空间静默生成.");

    private static readonly string AggregateRootInterface
        = "MicroserviceFramework.Domain.IAggregateRoot<";

    private static readonly string NonGenericAggregateRootInterface
        = "MicroserviceFramework.Domain.IAggregateRoot";

    /// <summary>
    /// 聚合根模型产出类别
    /// </summary>
    private enum GenerationKind
    {
        /// <summary>正常生成仓储接口</summary>
        Generate,

        /// <summary>全局命名空间：上报 MSF0001，不生成代码</summary>
        GlobalNamespace,

        /// <summary>命名空间不含约定段：上报 MSF0002 警告，不生成代码</summary>
        MissingAggregateSegment,
    }

    /// <summary>
    /// 聚合根的可等值比较模型：源生成器增量缓存的最小单位。
    /// 成员全部为字符串或可等值的结构（<see cref="TextSpan"/>/<see cref="LinePositionSpan"/>），
    /// 禁止把 <see cref="Location"/>（引用相等）等不可等值对象放进管线值：
    /// 那会让值相等缓存失效，每次编辑都触发下游重新生成
    /// </summary>
    /// <param name="Kind">产出类别</param>
    /// <param name="Namespace">聚合根所在命名空间；null 表示全局命名空间</param>
    /// <param name="ClassName">聚合根类型名（未转义，用于生成接口名）</param>
    /// <param name="Key">键类型显示名；空字符串表示无键（非泛型 IAggregateRoot，复合主键）</param>
    /// <param name="EscapedQualifiedName">关键字转义后的类型全名（如 <c>Ns.@event</c>），
    /// 用于 <c>using E = ...</c> 别名——未转义显示名会生成非法 using 代码</param>
    /// <param name="DiagnosticFilePath">诊断（全局命名空间/缺段）所在源文件路径</param>
    /// <param name="DiagnosticSourceSpan">诊断的文本区间（用于输出期重建 <see cref="Location"/>）</param>
    /// <param name="DiagnosticLineSpan">诊断的行列区间（用于输出期重建 <see cref="Location"/>）</param>
    private sealed record Model(
        GenerationKind Kind,
        string Namespace,
        string ClassName,
        string Key,
        string EscapedQualifiedName,
        string DiagnosticFilePath,
        TextSpan DiagnosticSourceSpan,
        LinePositionSpan DiagnosticLineSpan);

    /// <summary>
    /// 配置增量生成管线：
    /// ① SyntaxProvider 按声明语法（class/record class）提取聚合根模型（可等值 record，值相等驱动缓存）；
    /// ② Collect + 按 (Namespace, ClassName) 标识去重（partial 聚合根只保留首个声明）；
    /// ③ 输出期按 Kind 分流：正常生成仓储接口源码，或上报 MSF0001/MSF0002 诊断。
    /// 输出列表经 <see cref="EquatableList{T}"/> 包装，聚合根集合不变时整批跳过重新生成
    /// </summary>
    /// <param name="context">增量生成器初始化上下文</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 声明谓词同时命中 class 与 record class（record 聚合根同样实现 IAggregateRoot，
        // 旧实现只匹配 ClassDeclaration 会静默跳过 record）
        var pipeline = context.SyntaxProvider.CreateSyntaxProvider(
            static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax or RecordDeclarationSyntax,
            static (context, cancellationToken) =>
            {
                var typeDeclaration = (TypeDeclarationSyntax)context.Node;
                var assemblyClassTypeSymbol =
                    context.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);

                if (assemblyClassTypeSymbol == null)
                {
                    return null;
                }

                var key = ExtractKey(assemblyClassTypeSymbol);
                if (key == null)
                {
                    return null;
                }

                // 诊断位置以可等值数据（路径/文本区间/行列区间）存入模型，
                // 输出期重建 Location——Location 本身是引用相等对象，进入管线会毁掉值等值缓存
                var declaredLocation = typeDeclaration.GetLocation();
                var lineSpan = declaredLocation.GetLineSpan();

                // 全局命名空间（无 namespace）的类无法生成合法的仓储接口：
                // 命名空间段替换（*.AggregateRoots → *.Repositories）依赖命名空间存在，
                // 空命名空间会生成 "namespace ;" 非法代码。
                // 静默跳过会让用户只看到"没有生成仓储"而无从排查，
                // 故以编译错误诊断（MSF0001）显式上报，附类型声明的精确位置
                var @namespace = assemblyClassTypeSymbol.ContainingNamespace?.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                        SymbolDisplayGlobalNamespaceStyle.Omitted));
                if (string.IsNullOrEmpty(@namespace))
                {
                    return new Model(GenerationKind.GlobalNamespace, null,
                        assemblyClassTypeSymbol.Name, key, null,
                        lineSpan.Path, declaredLocation.SourceSpan, lineSpan.Span);
                }

                // 命名空间段约定校验（与文档一致，勿静默在任意命名空间生成）：
                // 不含 AggregateRoots/Aggregates 段的聚合根跳过生成并报 MSF0002 警告
                var hasAggregateSegment = @namespace.Split('.')
                    .Any(p => p is "AggregateRoots" or "Aggregates");
                if (!hasAggregateSegment)
                {
                    return new Model(GenerationKind.MissingAggregateSegment, @namespace,
                        assemblyClassTypeSymbol.Name, key, null,
                        lineSpan.Path, declaredLocation.SourceSpan, lineSpan.Span);
                }

                return new Model(GenerationKind.Generate, @namespace,
                    assemblyClassTypeSymbol.Name, key,
                    assemblyClassTypeSymbol.ToDisplayString(
                        new SymbolDisplayFormat(
                            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                            typeQualificationStyle:
                                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                            miscellaneousOptions:
                                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers)),
                    null, default, default);
            });

        // Collect + 标识去重后输出：
        // ① 同一聚合根的多个 partial 声明节点会产出多个 Model（诊断位置不同），
        //    以 (Namespace, ClassName) 标识去重只保留首个——否则同 hint name 重复 AddSource
        //    会生成重复类型（甚至触发同 hint name 异常），MSF0001 也会重复上报；
        // ② 输出列表为可等值包装（EquatableList），聚合根集合不变时整批跳过重新生成
        var distinctModels = pipeline.Collect().Select(static (models, _) =>
        {
            var seen = new HashSet<(string, string)>();
            var items = new List<Model>();
            foreach (var model in models)
            {
                if (model == null)
                {
                    continue;
                }

                if (seen.Add((model.Namespace ?? string.Empty, model.ClassName)))
                {
                    items.Add(model);
                }
            }

            return new EquatableList<Model>(items.ToArray());
        });

        context.RegisterSourceOutput(distinctModels, static (context, models) =>
        {
            foreach (var model in models.Items)
            {
                switch (model.Kind)
                {
                    case GenerationKind.GlobalNamespace:
                        context.ReportDiagnostic(Diagnostic.Create(
                            GlobalNamespaceAggregateRootRule,
                            Location.Create(model.DiagnosticFilePath, model.DiagnosticSourceSpan,
                                model.DiagnosticLineSpan),
                            model.ClassName));
                        continue;
                    case GenerationKind.MissingAggregateSegment:
                        context.ReportDiagnostic(Diagnostic.Create(
                            MissingAggregateSegmentRule,
                            Location.Create(model.DiagnosticFilePath, model.DiagnosticSourceSpan,
                                model.DiagnosticLineSpan),
                            model.ClassName, model.Namespace));
                        continue;
                }

                // 按命名空间段精确替换，避免 Replace 子串全量替换误伤其他段。
                // 与 MSFramework.Ef.Analyzers 的实现生成器保持同一约定：
                // 同时支持 "AggregateRoots" 与 "Aggregates" 两种领域段命名，
                // 否则聚合根放在 "Aggregates" 段时接口与实现会落到不同命名空间导致编译失败
                var repoNamespace = ReplaceNamespaceSegment(model.Namespace, "AggregateRoots", "Repositories");
                repoNamespace = ReplaceNamespaceSegment(repoNamespace, "Aggregates", "Repositories");
                // Key 为空表示实现非泛型 IAggregateRoot 的聚合根（复合主键多属性实体），生成无键仓储接口
                var baseInterfaces = string.IsNullOrEmpty(model.Key)
                    ? $"IRepository<E>"
                    : $"IRepository<E, {model.Key}>";
                var sourceText = SourceText.From($$"""
                                                   // <auto-generated />
                                                   using MicroserviceFramework.Domain;
                                                   using MicroserviceFramework.Extensions.DependencyInjection;
                                                   using E = {{model.EscapedQualifiedName}};

                                                   namespace {{repoNamespace}};

                                                   /// <summary>聚合根 <see cref="E"/> 的仓储契约，由框架生成基础操作与作用域注册标记。</summary>
                                                   public partial interface I{{model.ClassName}}Repository
                                                       : {{baseInterfaces}}, IScopeDependency
                                                   {
                                                   }
                                                   """, Encoding.UTF8);

                context.AddSource($"{repoNamespace}.{model.ClassName}.g.cs", sourceText);
            }
        });
    }

    /// <summary>
    /// 可等值的不可变列表包装：数组引用不变（或逐元素相等）即视为相等，
    /// 供增量生成器管线做缓存比较（原生数组/列表不提供值相等语义）
    /// </summary>
    /// <typeparam name="T">元素类型，须实现 <see cref="IEquatable{T}"/></typeparam>
    internal readonly struct EquatableList<T> : IEquatable<EquatableList<T>> where T : IEquatable<T>
    {
        private readonly T[] _items;

        /// <summary>
        /// 以数组初始化；null 视为空列表
        /// </summary>
        public EquatableList(T[] items)
        {
            _items = items ?? Array.Empty<T>();
        }

        /// <summary>
        /// 元素数组（只读）
        /// </summary>
        public T[] Items => _items;

        /// <summary>
        /// 判断两个列表是否等值：引用相同或长度与逐元素相等均一致
        /// </summary>
        public bool Equals(EquatableList<T> other)
        {
            if (ReferenceEquals(_items, other._items))
            {
                return true;
            }

            if (_items.Length != other._items.Length)
            {
                return false;
            }

            for (var i = 0; i < _items.Length; i++)
            {
                if (!_items[i].Equals(other._items[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is EquatableList<T> other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = 17;
            foreach (var item in _items)
            {
                hash = hash * 31 + (item?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }

    /// <summary>
    /// 按命名空间段精确替换，避免 Replace 子串全量替换误伤其他包含该子串的段
    /// </summary>
    /// <param name="fullName">完整命名空间</param>
    /// <param name="segment">待替换的命名空间段</param>
    /// <param name="replacement">替换后的命名空间段</param>
    /// <returns>替换后的命名空间</returns>
    private static string ReplaceNamespaceSegment(string fullName, string segment, string replacement)
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

    /// <summary>
    /// 从类型实现的接口中提取聚合根键类型名。
    /// 优先匹配泛型 <see cref="IAggregateRoot{TKey}"/>（返回键类型名）；
    /// 若仅实现非泛型 <see cref="IAggregateRoot"/>（复合主键多属性实体），返回空字符串表示无键；
    /// 两者均未实现返回 null（忽略该类型）。
    /// </summary>
    /// <param name="typeSymbol">待检查的类型符号</param>
    /// <returns>键类型名；空字符串表示无键聚合根；null 表示非聚合根</returns>
    private static string ExtractKey(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol
            {
                TypeKind: TypeKind.Class,
                IsAbstract: false,
                IsGenericType: false,
                ContainingType: null,
                DeclaredAccessibility: Accessibility.Public
            })
        {
            return null;
        }

        var hasGenericAggregateRoot = false;
        string key = null;
        foreach (var @interface in typeSymbol.AllInterfaces)
        {
            var interfaceName = @interface.ToDisplayString();
            if (!interfaceName.StartsWith(AggregateRootInterface))
            {
                continue;
            }

            hasGenericAggregateRoot = true;
            key = interfaceName.Substring(
                AggregateRootInterface.Length,
                interfaceName.Length - AggregateRootInterface.Length - 1);
            break;
        }

        if (hasGenericAggregateRoot)
        {
            return key;
        }

        // 泛型接口未命中时，检查是否实现非泛型 IAggregateRoot（复合主键多属性实体）
        foreach (var @interface in typeSymbol.AllInterfaces)
        {
            if (@interface.ToDisplayString() == NonGenericAggregateRootInterface)
            {
                return string.Empty;
            }
        }

        return null;
    }
}
