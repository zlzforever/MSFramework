using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MicroserviceFramework.Ef.Analyzers;

/// <summary>
/// 仓储 EF<b>实现</b>源生成器（Infrastructure 层）：
/// 扫描引用的 Domain 程序集（程序集名经 Infrastructure→Domain 段映射反推）中实现了
/// <see cref="IAggregateRoot{TKey}"/>（或非泛型 <see cref="IAggregateRoot"/>）的公共类，
/// 按命名空间段约定生成 <c>{X}Repository : EfRepository&lt;E, TKey&gt;, I{X}Repository</c>
/// 部分类实现，与 MSFramework.Analyzers 的 <c>RepositoryInterfaceGenerator</c>（Domain 层接口）
/// 成对工作——接口未生成的类型本生成器同样跳过（缺段 MSF0002），保证成对一致。
/// 增量管线以可等值模型（全字符串/结构 record 列表）为缓存边界，
/// 聚合根集合不变时整批跳过重新生成
/// </summary>
[Generator(LanguageNames.CSharp)]
public class RepositoryGenerator : IIncrementalGenerator
{
    /// <summary>
    /// 全局命名空间聚合根的诊断规则：无法生成仓储实现，以编译错误显式上报
    /// </summary>
    private static readonly DiagnosticDescriptor GlobalNamespaceAggregateRootRule = new(
        "MSF0001",
        "全局命名空间的聚合根无法生成仓储实现",
        "聚合根 '{0}' 位于全局命名空间（无 namespace），无法生成仓储实现：" +
        "请为类型添加显式命名空间，且命名空间需包含 'AggregateRoots'（或 'Aggregates'）段",
        "MSFramework.SourceGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "仓储实现由源生成器按命名空间段（Domain → Infrastructure）推导生成，" +
        "聚合根必须位于显式命名空间中；全局命名空间的类型会被跳过，这里显式报错以便定位.");

    /// <summary>
    /// 聚合根未放在约定命名空间段的诊断规则：与接口生成器（MSFramework.Analyzers）同一约定，
    /// 跳过生成并以警告显式上报——否则接口生成器不产出仓储接口时，
    /// 实现生成器仍会生成孤儿实现导致编译失败
    /// </summary>
    private static readonly DiagnosticDescriptor MissingAggregateSegmentRule = new(
        "MSF0002",
        "聚合根未放在约定的命名空间段下，未生成仓储实现",
        "类型 '{0}' 实现了 IAggregateRoot 但命名空间 '{1}' 不包含 'AggregateRoots'（或 'Aggregates'）段，" +
        "未生成仓储实现：请将聚合根移入 *.AggregateRoots（或 *.Aggregates）命名空间段",
        "MSFramework.SourceGeneration",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "仓储接口/实现的生成依赖命名空间段约定（AggregateRoots/Aggregates → Repositories、" +
        "Domain → Infrastructure）；不在约定段下的聚合根会被跳过，避免在任意命名空间静默生成.");

    /// <summary>
    /// 聚合根模型产出类别
    /// </summary>
    private enum GenerationKind
    {
        /// <summary>正常生成仓储实现</summary>
        Generate,

        /// <summary>全局命名空间：上报 MSF0001，不生成代码</summary>
        GlobalNamespace,

        /// <summary>命名空间不含约定段：上报 MSF0002 警告，不生成代码</summary>
        MissingAggregateSegment,
    }

    /// <summary>
    /// 单个聚合根的可等值比较模型：源生成器增量缓存的最小单位。
    /// 成员全部为字符串或可等值的结构（<see cref="TextSpan"/>/<see cref="LinePositionSpan"/>），
    /// 聚合根集合未变化时两次编辑产出等值模型，下游 <see cref="RegisterSourceOutput"/> 可整体跳过。
    /// 禁止把 <see cref="Compilation"/>/<see cref="Location"/>（引用相等）等不可等值对象放进管线值：
    /// 那会让每次按键都触发全量重扫，增量缓存形同虚设
    /// </summary>
    private sealed record AggregateRootModel(
        string SourceText,
        string HintName,
        string TypeName,
        GenerationKind Kind,
        string DiagnosticNamespace,
        string DiagnosticFilePath,
        TextSpan DiagnosticSourceSpan,
        LinePositionSpan DiagnosticLineSpan);

    /// <summary>
    /// 配置增量生成管线：CompilationProvider 投影为可等值的聚合根模型列表
    /// （<see cref="AggregateRootModel"/>；禁止把 <see cref="Compilation"/> 等不可等值对象
    /// 直接放进管线值——那会让每次按键都全量重扫、增量缓存形同虚设），
    /// 输出期按 Kind 分流：正常生成仓储实现源码，或上报 MSF0001/MSF0002 诊断
    /// </summary>
    /// <param name="context">增量生成器初始化上下文</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 增量管线：CompilationProvider 的投影输出为可等值比较的模型列表（EquatableList）。
        // 旧实现直接把 Compilation 传给 RegisterSourceOutput——每次编辑 Compilation 都是新实例，
        // 等值比较恒不相等，导致每次按键都全量重跑符号遍历（增量形同虚设）；
        // 现在只在聚合根集合实际变化时才重新生成源码
        var models = context.CompilationProvider
            .Select(static (compilation, _) => ComputeModels(compilation));
        context.RegisterSourceOutput(models, static (context, models) =>
        {
            foreach (var model in models.Items)
            {
                switch (model.Kind)
                {
                    case GenerationKind.GlobalNamespace:
                        // 全局命名空间（显示名无 '.' 分隔）的聚合根无法生成合法的仓储实现：
                        // 模板会生成 "using {TypeName};"（把类型名当命名空间）等非法代码。
                        // 静默跳过会让用户只看到"没有生成仓储实现"而无从排查，
                        // 故以编译错误诊断（MSF0001）显式上报；位置为域内类型的元数据位置
                        // （Location 由可等值的路径/文本区间/行列区间在输出期重建）
                        context.ReportDiagnostic(Diagnostic.Create(
                            GlobalNamespaceAggregateRootRule,
                            Location.Create(model.DiagnosticFilePath, model.DiagnosticSourceSpan,
                                model.DiagnosticLineSpan),
                            model.TypeName));
                        continue;
                    case GenerationKind.MissingAggregateSegment:
                        // 与接口生成器同一约定：缺段聚合根报警告并跳过，
                        // 保持"接口未生成 → 实现也不生成"的成对一致
                        context.ReportDiagnostic(Diagnostic.Create(
                            MissingAggregateSegmentRule,
                            Location.Create(model.DiagnosticFilePath, model.DiagnosticSourceSpan,
                                model.DiagnosticLineSpan),
                            model.TypeName, model.DiagnosticNamespace));
                        continue;
                }

                context.AddSource(model.HintName,
                    SourceText.From(model.SourceText, Encoding.UTF8));
            }
        });
    }

    /// <summary>
    /// 从编译中提取全部聚合根的可等值生成模型（源文本、提示名、诊断信息）。
    /// 输入不可等值的 <see cref="Compilation"/>，输出全部由可等值数据构成，
    /// 作为增量管线的缓存边界
    /// </summary>
    /// <param name="compilation">当前编译</param>
    /// <returns>聚合根模型列表</returns>
    private static EquatableList<AggregateRootModel> ComputeModels(Compilation compilation)
    {
        // 精确匹配程序集名（不包含扩展名），避免 Contains 子串误匹配
        var domainAssemblyName = Extensions.ReplaceNamespaceSegment(
            compilation.AssemblyName ?? string.Empty, "Infrastructure", "Domain");
        if (string.IsNullOrEmpty(domainAssemblyName))
        {
            return new EquatableList<AggregateRootModel>(Array.Empty<AggregateRootModel>());
        }

        var domainAssemblySymbol = compilation.SourceModule.ReferencedAssemblySymbols
            .FirstOrDefault(x => x.Name == domainAssemblyName);
        if (domainAssemblySymbol == null)
        {
            return new EquatableList<AggregateRootModel>(Array.Empty<AggregateRootModel>());
        }

        var list = LoadAllTypes(domainAssemblySymbol.GlobalNamespace);
        var models = new List<AggregateRootModel>();

        foreach (var typeSymbol in list)
        {
            // 直接消费类型符号（不再经"显示名 → GetTypeByMetadataName"回查）：
            // 关键字命名的聚合根（如 @event）的转义显示名会被按元数据名查找拒收，
            // 导致该类型静默漏生成仓储实现且无任何诊断
            var aggregateRootInfo = typeSymbol.GetAggregateRootInfo();
            if (!aggregateRootInfo.IsAggregateRoot)
            {
                continue;
            }

            // 全局命名空间（无 namespace）的聚合根无法生成合法的仓储实现：
            // 模板会生成 "using {TypeName};"（把类型名当命名空间）等非法代码，
            // 以编译错误诊断（MSF0001）显式上报
            if (aggregateRootInfo.Type.ContainingNamespace is null or { IsGlobalNamespace: true })
            {
                // 诊断位置取类型定义位置的路径、文本区间与行列区间（全部可等值）
                var location = aggregateRootInfo.Type.Locations.FirstOrDefault();
                var lineSpan = location?.GetLineSpan() ?? default;

                models.Add(new AggregateRootModel(
                    SourceText: null,
                    HintName: null,
                    TypeName: aggregateRootInfo.Type.Name,
                    Kind: GenerationKind.GlobalNamespace,
                    DiagnosticNamespace: null,
                    DiagnosticFilePath: lineSpan.Path ?? string.Empty,
                    DiagnosticSourceSpan: location?.SourceSpan ?? default,
                    DiagnosticLineSpan: lineSpan.Span));
                continue;
            }

            // 命名空间段约定校验（与接口生成器同一约定）：
            // 不含 AggregateRoots/Aggregates 段的聚合根跳过生成并报 MSF0002 警告，
            // 避免"接口生成器不产出仓储接口、实现生成器却生成孤儿实现"的成对失配
            var aggregateNamespace =
                aggregateRootInfo.Type.ContainingNamespace.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                        SymbolDisplayGlobalNamespaceStyle.Omitted));
            var hasAggregateSegment = aggregateNamespace.Split('.')
                .Any(p => p is "AggregateRoots" or "Aggregates");
            if (!hasAggregateSegment)
            {
                var location = aggregateRootInfo.Type.Locations.FirstOrDefault();
                var lineSpan = location?.GetLineSpan() ?? default;

                models.Add(new AggregateRootModel(
                    SourceText: null,
                    HintName: null,
                    TypeName: aggregateRootInfo.Type.Name,
                    Kind: GenerationKind.MissingAggregateSegment,
                    DiagnosticNamespace: aggregateNamespace,
                    DiagnosticFilePath: lineSpan.Path ?? string.Empty,
                    DiagnosticSourceSpan: location?.SourceSpan ?? default,
                    DiagnosticLineSpan: lineSpan.Span));
                continue;
            }

            var typeInfo = aggregateRootInfo.Type.GetTypeInfo();
            var namespaceName = Extensions.ReplaceNamespaceSegment(typeInfo.Namespace, "Domain", "Infrastructure");
            // 别名必须转义关键字：聚合根名经 @ 转义定义（如 @class）时，
            // 未转义显示名会生成 "using E = Ns.class;" 非法代码
            var escapedEntity = aggregateRootInfo.Type.ToDisplayString(
                new SymbolDisplayFormat(
                    globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                    typeQualificationStyle:
                        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers));
            // Key 为空表示实现非泛型 IAggregateRoot 的聚合根（复合主键多属性实体），生成无键仓储
            var baseType = string.IsNullOrEmpty(aggregateRootInfo.Key)
                ? "EfRepository<E>"
                : $"EfRepository<E, {aggregateRootInfo.Key}>";
            var script = $$"""
                           // <auto-generated />
                           using MicroserviceFramework.Ef;
                           using MicroserviceFramework.Ef.Repositories;
                           using {{typeInfo.Namespace}};
                           using E = {{escapedEntity}};

                           namespace {{namespaceName}};

                           /// <summary>聚合根 <see cref="E"/> 的 EF 仓储实现，依赖当前作用域上下文工厂。</summary>
                           public partial class {{typeInfo.TypeName}}Repository
                               : {{baseType}}, I{{typeInfo.TypeName}}Repository
                           {
                               /// <summary>使用实体所属的作用域上下文创建仓储。</summary>
                               /// <param name="context">框架上下文工厂，须已注册实体映射。</param>
                               public {{typeInfo.TypeName}}Repository(DbContextFactory context) : base(context)
                               {
                               }
                           }
                           """;

            models.Add(new AggregateRootModel(
                SourceText: script,
                HintName: $"{typeInfo.Namespace}.{typeInfo.TypeName}.g.cs",
                TypeName: typeInfo.TypeName,
                Kind: GenerationKind.Generate,
                DiagnosticNamespace: null,
                DiagnosticFilePath: null,
                DiagnosticSourceSpan: default,
                DiagnosticLineSpan: default));
        }

        return new EquatableList<AggregateRootModel>(models.ToArray());
    }

    /// <summary>
    /// 可等值的不可变列表包装：数组引用不变（或逐元素相等）即视为相等，
    /// 供增量生成器管线做缓存比较（<see cref="ImmutableArray{T}"/> 等原生类型不提供值相等语义）
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
    /// 递归收集命名空间下所有类型成员的<b>类型符号</b>集合。
    /// <para>
    /// 旧实现收集 <c>ToDisplayString()</c> 显示名、再经 <see cref="Compilation.GetTypeByMetadataName"/>
    /// 回查符号——显示名是<b>转义</b>的（如 <c>Ns.@event</c>），而元数据名要求<b>非转义</b>（如 <c>Ns.event</c>），
    /// 关键字命名的聚合根会被静默拒收、漏生成仓储实现且无任何诊断；直接传符号消除该失败模式
    /// </para>
    /// </summary>
    /// <param name="namespaceSymbol">命名空间符号</param>
    /// <returns>候选类型符号集合（非抽象、非泛型定义、非嵌套的公共类）</returns>
    private static List<INamedTypeSymbol> LoadAllTypes(INamespaceSymbol namespaceSymbol)
    {
        var list = new List<INamedTypeSymbol>();
        foreach (var namespaceMember in namespaceSymbol
                     .GetNamespaceMembers())
        {
            list.AddRange(LoadAllTypes(namespaceMember));
        }

        foreach (var member in namespaceSymbol.GetMembers())
        {
            // 正向模式匹配并绑定符号：避免依赖 is-not 的类型收窄
            // （member 的静态类型是 INamespaceOrTypeSymbol，部分编译器版本对此收窄不可靠）
            if (member is INamedTypeSymbol
                {
                    TypeKind: TypeKind.Class,
                    IsAbstract: false,
                    IsGenericType: false,
                    ContainingType: null,
                    DeclaredAccessibility: Accessibility.Public
                } namedType)
            {
                list.Add(namedType);
            }
        }

        return list;
    }
}
