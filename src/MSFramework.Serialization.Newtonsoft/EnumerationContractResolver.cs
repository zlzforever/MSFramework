using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

/// <summary>
///     为 Enumeration 子类型自动分配 EnumerationConverter 的合约解析器
/// </summary>
public class EnumerationContractResolver : DefaultContractResolver
{
    /// <summary>
    ///     创建合约，若类型为 Enumeration 子类则将转换器设置为 EnumerationConverter
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
