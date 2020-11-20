using System;
using System.Linq;
using System.Reflection;

namespace MicroserviceFramework
{
	public class MicroserviceFrameworkLoader
	{
		public static event Action<Type> RegisterType;

		public static void RegisterTypes()
		{
			if (RegisterType == null)
			{
				return;
			}

			var entryAssembly = Assembly.GetEntryAssembly();
			if (entryAssembly != null)
			{
				var pre = entryAssembly.GetName().Name.Split('.').FirstOrDefault();
				AppDomain.CurrentDomain.Load($"{pre}.Application");
			}

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					RegisterType(type);
				}
			}
		}
	}
}