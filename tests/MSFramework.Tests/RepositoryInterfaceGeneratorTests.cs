using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 仓储接口源生成器测试：
/// 聚合根位于全局命名空间（无 namespace）时生成 MSF0001 编译错误并跳过代码生成；
/// 位于显式命名空间时正常生成 <c>I{X}Repository</c> 接口且无诊断。
/// </summary>
public class RepositoryInterfaceGeneratorTests
{
    /// <summary>
    /// 构建测试编译单元：引用运行时与 MSFramework 主程序集，
    /// 使聚合根接口（IAggregateRoot）可被语义模型解析
    /// </summary>
    /// <param name="source">待编译的 C# 源码</param>
    /// <returns>编译单元</returns>
    private static CSharpCompilation CreateCompilation(string source)
    {
        var references = new List<MetadataReference>
        {
            // System.Private.CoreLib：基础类型解析
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            // MSFramework：IAggregateRoot 及其约束类型
            MetadataReference.CreateFromFile(
                typeof(MicroserviceFramework.Domain.IAggregateRoot<>).Assembly.Location)
        };

        return CSharpCompilation.Create("TestDomain",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>
    /// 运行生成器并返回生成结果
    /// </summary>
    /// <param name="compilation">编译单元</param>
    /// <returns>生成器运行结果</returns>
    private static GeneratorDriverRunResult RunGenerator(CSharpCompilation compilation)
    {
        var driver = CSharpGeneratorDriver
            .Create(new MicroserviceFramework.Analyzers.RepositoryInterfaceGenerator())
            .RunGenerators(compilation);
        return driver.GetRunResult();
    }

    /// <summary>
    /// 全局命名空间聚合根：生成 MSF0001 错误（含精确位置）且不生成仓储接口
    /// </summary>
    [Fact]
    public void GlobalNamespaceAggregateRoot_ReportsMsf0001ErrorAndSkipsGeneration()
    {
        var compilation = CreateCompilation("""
                                            using MicroserviceFramework.Domain;

                                            public class GlobalAggregate : IAggregateRoot<int>
                                            {
                                                public int Id { get; }
                                                public GlobalAggregate(int id) { Id = id; }
                                            }
                                            """);

        var result = RunGenerator(compilation);

        var diagnostic = Assert.Single(
            result.Diagnostics.Where(x => x.Id == "MSF0001"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("GlobalAggregate", diagnostic.GetMessage());
        // 诊断应定位到类型声明行（源码第 3 行，0 起始为 2）
        var span = diagnostic.Location.GetLineSpan();
        Assert.Equal(2, span.StartLinePosition.Line);

        // 不生成任何仓储接口代码
        Assert.All(result.GeneratedTrees, tree => Assert.False(
            tree.FilePath.Contains("IRepository", StringComparison.OrdinalIgnoreCase) &&
            tree.FilePath.Contains("GlobalAggregate"),
            "全局命名空间聚合根不应生成仓储接口"));
    }

    /// <summary>
    /// 聚合根类型名为 C# 关键字（经 @ 转义声明）：生成的 using 别名必须转义（Ns.@event），
    /// 否则生成 "using E = Ns.event;" 非法代码
    /// </summary>
    [Fact]
    public void KeywordNamedAggregateRoot_EscapesAliasInGeneratedCode()
    {
        var compilation = CreateCompilation("""
                                            using MicroserviceFramework.Domain;

                                            namespace Test.Domain.AggregateRoots;

                                            public class @event : IAggregateRoot<int>
                                            {
                                                public int Id { get; }
                                                public @event(int id) { Id = id; }
                                            }
                                            """);

        var result = RunGenerator(compilation);

        Assert.Empty(result.Diagnostics.Where(x => x.Id == "MSF0001"));

        var generated = Assert.Single(result.GeneratedTrees);
        var text = generated.GetText().ToString();
        // 别名按转义显示名生成
        Assert.Contains("using E = Test.Domain.AggregateRoots.@event;", text);
        // 接口名由"类型名 + Repository 后缀"组成，不再是关键字
        Assert.Contains("public partial interface IeventRepository", text);
    }

    /// <summary>
    /// 显式命名空间（含 AggregateRoots 段）聚合根：正常生成仓储接口且无 MSF0001
    /// </summary>
    [Fact]
    public void NamespaceAggregateRoot_GeneratesRepositoryInterfaceWithoutDiagnostic()
    {
        var compilation = CreateCompilation("""
                                            using MicroserviceFramework.Domain;

                                            namespace Test.Domain.AggregateRoots;

                                            public class Product : IAggregateRoot<int>
                                            {
                                                public int Id { get; }
                                                public Product(int id) { Id = id; }
                                            }
                                            """);

        var result = RunGenerator(compilation);

        Assert.Empty(result.Diagnostics.Where(x => x.Id == "MSF0001"));

        // 生成 IProductRepository 接口，且命名空间段替换为 Repositories
        var generated = Assert.Single(result.GeneratedTrees);
        var text = generated.GetText().ToString();
        Assert.Contains("namespace Test.Domain.Repositories;", text);
        Assert.Contains("public partial interface IProductRepository", text);
        Assert.Contains("IRepository<E, int>", text);
        // 按完整行断言，防止原始字符串模板意外保留顶层声明的前导空格。
        Assert.Equal(new[]
        {
            "// <auto-generated />",
            "using MicroserviceFramework.Domain;",
            "using MicroserviceFramework.Extensions.DependencyInjection;",
            "using E = Test.Domain.AggregateRoots.Product;",
            "",
            "namespace Test.Domain.Repositories;",
            "",
            "/// <summary>聚合根 <see cref=\"E\"/> 的仓储契约，由框架生成基础操作与作用域注册标记。</summary>",
            "public partial interface IProductRepository",
            "    : IRepository<E, int>, IScopeDependency",
            "{",
            "}"
        }, text.Replace("\r\n", "\n").Split('\n'));
    }

    /// <summary>
    /// record class 声明的聚合根：同样生成仓储接口（旧实现只匹配 ClassDeclaration 会静默跳过 record）
    /// </summary>
    [Fact]
    public void RecordAggregateRoot_GeneratesRepositoryInterface()
    {
        var compilation = CreateCompilation("""
                                            using MicroserviceFramework.Domain;

                                            namespace Test.Domain.AggregateRoots;

                                            public record class Product : IAggregateRoot<int>
                                            {
                                                public int Id { get; }
                                                public Product(int id) { Id = id; }
                                            }
                                            """);

        var result = RunGenerator(compilation);

        Assert.Empty(result.Diagnostics.Where(x => x.Id == "MSF0001" || x.Id == "MSF0002"));

        var generated = Assert.Single(result.GeneratedTrees);
        var text = generated.GetText().ToString();
        Assert.Contains("public partial interface IProductRepository", text);
    }

    /// <summary>
    /// 聚合根未放在约定命名空间段（如 *.Models）：报 MSF0002 警告且不生成仓储接口
    /// （与文档约定一致，避免在任意命名空间静默生成造成误生成/命名冲突）
    /// </summary>
    [Fact]
    public void MissingAggregateSegment_ReportsMsf0002WarningAndSkipsGeneration()
    {
        var compilation = CreateCompilation("""
                                            using MicroserviceFramework.Domain;

                                            namespace Test.Domain.Models;

                                            public class Product : IAggregateRoot<int>
                                            {
                                                public int Id { get; }
                                                public Product(int id) { Id = id; }
                                            }
                                            """);

        var result = RunGenerator(compilation);

        var diagnostic = Assert.Single(
            result.Diagnostics.Where(x => x.Id == "MSF0002"));
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("Test.Domain.Models", diagnostic.GetMessage());
        Assert.Empty(result.GeneratedTrees);
    }

    /// <summary>
    /// partial 聚合根（同一类型的多个声明节点）：只生成一份源码，
    /// 不因同 hint name 重复 AddSource 而冲突，MSF0001 也不重复上报
    /// </summary>
    [Fact]
    public void PartialAggregateRoot_GeneratesSingleSource()
    {
        var compilation = CreateCompilation("""
                                            using MicroserviceFramework.Domain;

                                            namespace Test.Domain.AggregateRoots;

                                            public partial class Product : IAggregateRoot<int>
                                            {
                                                public int Id { get; }
                                                public Product(int id) { Id = id; }
                                            }

                                            public partial class Product
                                            {
                                            }
                                            """);

        var result = RunGenerator(compilation);

        Assert.Empty(result.Diagnostics.Where(x => x.Id == "MSF0001" || x.Id == "MSF0002"));
        var generated = Assert.Single(result.GeneratedTrees);
        var text = generated.GetText().ToString();
        Assert.Contains("public partial interface IProductRepository", text);
    }
}
