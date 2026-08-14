using MicroserviceFramework.Serialization.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace MSFramework.Tests;

public class CompositeContractResolverTests
{
    /// <summary>
    /// 用于验证 camelCase 序列化生效的测试对象
    /// </summary>
    private class CamelTestObject
    {
        public string UserName { get; set; }
    }

    [Fact]
    public void ResolveContract_AppliesCamelCase_ForNonEnumerationType()
    {
        // 回归测试：旧实现永远命中第一个 resolver（EnumerationContractResolver），
        // camelCase 从未生效；新实现非 Enumeration 类型必须回退到 camelCase resolver
        var resolver = new CompositeContractResolver
        {
            new EnumerationContractResolver(), new CamelCasePropertyNamesContractResolver()
        };

        var json = JsonConvert.SerializeObject(new CamelTestObject { UserName = "lewis" },
            new JsonSerializerSettings { ContractResolver = resolver });

        Assert.Contains("\"userName\"", json);
        Assert.DoesNotContain("\"UserName\"", json);
    }

    [Fact]
    public void ResolveContract_ReturnsContract_WhenNoResolverMatches()
    {
        // 全部 resolver 均未命中时回退默认合约，避免返回 null 导致 Newtonsoft 崩溃
        var resolver = new CompositeContractResolver { new EnumerationContractResolver() };

        var contract = resolver.ResolveContract(typeof(CamelTestObject));

        Assert.NotNull(contract);
    }
}
