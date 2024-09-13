using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

/// <summary>
///
/// </summary>
public class EnumerationContractResolver : DefaultContractResolver
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="objectType"></param>
    /// <returns></returns>
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
