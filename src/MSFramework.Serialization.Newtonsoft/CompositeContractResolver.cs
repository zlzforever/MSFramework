using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

/// <summary>
///
/// </summary>
public class CompositeContractResolver : IContractResolver, IEnumerable<IContractResolver>
{
    private readonly IList<IContractResolver> _contractResolvers = new List<IContractResolver>();

    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public JsonContract ResolveContract(Type type)
    {
        return
            _contractResolvers
                .Select(x => x.ResolveContract(type))
                .FirstOrDefault();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="contractResolver"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Add(IContractResolver contractResolver)
    {
        if (contractResolver == null) throw new ArgumentNullException(nameof(contractResolver));
        _contractResolvers.Add(contractResolver);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public IEnumerator<IContractResolver> GetEnumerator()
    {
        return _contractResolvers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
