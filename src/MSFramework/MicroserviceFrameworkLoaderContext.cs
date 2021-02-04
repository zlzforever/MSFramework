using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MicroserviceFramework.Utilities;

namespace MicroserviceFramework
{
	public class MicroserviceFrameworkLoaderContext
	{
		public static readonly MicroserviceFrameworkLoaderContext Default = new();

		private MicroserviceFrameworkLoaderContext()
		{
		}

		public event Action<Type> ResolveType;

		public void LoadTypes()
		{
			if (ResolveType == null)
			{
				return;
			}

			var types = RuntimeUtilities.GetAllTypes();

			foreach (var type in types)
			{
				ResolveType(type);
			}
		}
	}
}