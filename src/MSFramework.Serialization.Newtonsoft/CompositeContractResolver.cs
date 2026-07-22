using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

/// <summary>
///     组合多个 IContractResolver 的合成解析器，按添加顺序依次尝试解析合约
/// </summary>
public class CompositeContractResolver : IContractResolver, IEnumerable<IContractResolver>
{
    private readonly IList<IContractResolver> _contractResolvers = new List<IContractResolver>();

    /// <summary>
    ///     按顺序尝试每个合约解析器，返回第一个成功的合约
    /// </summary>
    /// <param name="type">对象类型</param>
    /// <returns>JSON 合约</returns>
    public JsonContract ResolveContract(Type type)
    {
        return
            _contractResolvers
                .Select(x => x.ResolveContract(type))
                .FirstOrDefault();
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
