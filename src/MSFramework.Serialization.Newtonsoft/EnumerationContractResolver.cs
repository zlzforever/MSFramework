using System;
using System.Collections;
using System.Collections.Generic;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

/// <summary>
///     为 Enumeration 子类型自动分配 EnumerationConverter 的合约解析器。
///     非 Enumeration 类型返回 null，以便组合解析器（<see cref="CompositeContractResolver"/>）
///     继续尝试后续解析器，保证 camelCase 等通用解析规则生效。
/// </summary>
public class EnumerationContractResolver : DefaultContractResolver
{
    /// <summary>
    ///     创建合约，仅当类型为 Enumeration 子类时返回合约并将转换器设置为 EnumerationConverter；
    ///     其他类型返回 null，交由组合解析器中的下一个解析器处理
    /// </summary>
    /// <param name="objectType">对象类型</param>
    /// <returns>JSON 合约；非 Enumeration 子类型返回 null</returns>
    public override JsonContract ResolveContract(Type objectType)
    {
        if (objectType == null || !objectType.IsSubclassOf(typeof(Enumeration)))
        {
            return null;
        }

        return base.ResolveContract(objectType);
    }

    /// <summary>
    ///     为 Enumeration 子类合约附加 EnumerationConverter
    /// </summary>
    /// <param name="objectType">对象类型</param>
    /// <returns>JSON 合约</returns>
    protected override JsonContract CreateContract(Type objectType)
    {
        var contract = base.CreateContract(objectType);

        // this will only be called once and then cached
        if (objectType.IsSubclassOf(typeof(Enumeration)))
        {
            contract.Converter = new EnumerationConverter();
        }

        return contract;
    }
}
