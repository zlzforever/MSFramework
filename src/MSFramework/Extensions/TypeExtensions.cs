using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MSFramework.Extensions
{
	public static class TypeExtensions
	{
		public static Type[] GetImplementedInterfaces(this Type type, params Type[] exceptInterfaces)
		{
			var interfaceTypes = type.GetInterfaces().Where(t => !exceptInterfaces.Contains(t)).ToArray();
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