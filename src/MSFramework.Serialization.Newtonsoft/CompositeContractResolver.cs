﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Newtonsoft
{
	public class CompositeContractResolver : IContractResolver, IEnumerable<IContractResolver>
	{
		private readonly IList<IContractResolver> _contractResolvers = new List<IContractResolver>();

		public JsonContract ResolveContract(Type type)
		{
			return
				_contractResolvers
					.Select(x => x.ResolveContract(type))
					.FirstOrDefault();
		}

		public void Add(IContractResolver contractResolver)
		{
			if (contractResolver == null) throw new ArgumentNullException(nameof(contractResolver));
			_contractResolvers.Add(contractResolver);
		}

		public IEnumerator<IContractResolver> GetEnumerator()
		{
			return _contractResolvers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}