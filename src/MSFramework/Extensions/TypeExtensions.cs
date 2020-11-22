using System;
using System.Linq;

namespace MicroserviceFramework.Extensions
{
	public static class TypeExtensions
	{
		public static Type[] GetInterfaces(this Type type, params Type[] excludeInterfaces)
		{
			var interfaceTypes = type.GetInterfaces()
				.Where(t => !excludeInterfaces.Contains(t))
				.ToArray();
			for (var index = 0; index < interfaceTypes.Length; index++)
			{
				var interfaceType = interfaceTypes[index];
				if (interfaceType.IsGenericType && !interfaceType.IsGenericTypeDefinition &&
				    interfaceType.FullName == null)
				{
					interfaceTypes[index] = interfaceType.GetGenericTypeDefinition();
				}
			}

			return interfaceTypes;
		}
	}
}