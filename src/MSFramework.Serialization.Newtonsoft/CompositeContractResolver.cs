using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

/// <summary>
///     组合多个 IContractResolver 的合成解析器，按添加顺序返回第一个「非 null」合约。
///     解析器对不匹配的类型应返回 null（如 <see cref="EnumerationContractResolver"/>），
///     以便后续解析器（如 camelCase）生效；所有解析器均未命中时回退默认合约。
/// </summary>
public class CompositeContractResolver : IContractResolver, IEnumerable<IContractResolver>
{
    private static readonly DefaultContractResolver FallbackResolver = new();
    private readonly IList<IContractResolver> _contractResolvers = new List<IContractResolver>();

    /// <summary>
    ///     按顺序尝试每个合约解析器，返回第一个非 null 的合约；
    ///     全部未命中时回退到 <see cref="DefaultContractResolver"/>，避免返回 null
    /// </summary>
    /// <param name="type">对象类型</param>
    /// <returns>JSON 合约</returns>
    public JsonContract ResolveContract(Type type)
    {
        foreach (var contractResolver in _contractResolvers)
        {
            var contract = contractResolver.ResolveContract(type);
            if (contract != null)
            {
                return contract;
            }
        }

        return FallbackResolver.ResolveContract(type);
    }

    /// <summary>
    ///     添加一个合约解析器到组合列表
    /// </summary>
    /// <param name="contractResolver">合约解析器</param>
    /// <exception cref="ArgumentNullException">contractResolver 为 null 时抛出</exception>
    public void Add(IContractResolver contractResolver)
    {
        if (contractResolver == null) throw new ArgumentNullException(nameof(contractResolver));
        _contractResolvers.Add(contractResolver);
    }

    /// <summary>
    ///     返回合约解析器的枚举器
    /// </summary>
    /// <returns>合约解析器枚举器</returns>
    public IEnumerator<IContractResolver> GetEnumerator()
    {
        return _contractResolvers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
