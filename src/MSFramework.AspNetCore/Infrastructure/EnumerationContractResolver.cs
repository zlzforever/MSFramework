using System;
using MSFramework.Domain;
using Newtonsoft.Json.Serialization;

namespace MSFramework.AspNetCore.Infrastructure
{
	public class EnumerationContractResolver: DefaultContractResolver
	{
		protected override JsonContract CreateContract(Type objectType)
		{
			JsonContract contract = base.CreateContract(objectType);

			// this will only be called once and then cached
			if (objectType.IsSubclassOf(typeof(Enumeration)))
			{
				contract.Converter = new EnumerationConverter();
			}

			return contract;
		}
	}
}