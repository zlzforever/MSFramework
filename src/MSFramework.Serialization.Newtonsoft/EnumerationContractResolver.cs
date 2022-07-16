﻿using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Newtonsoft.Converters;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Newtonsoft
{
	public class EnumerationContractResolver : DefaultContractResolver
	{
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
}