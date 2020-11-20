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

		public static string GetGenericTypeName(this Type type)
		{
			string typeName;

			if (type.IsGenericType)
			{
				var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
				typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
			}
			else
			{
				typeName = type.Name;
			}

			return typeName;
		}

		public static string GetGenericTypeName(this object @object)
		{
			return @object.GetType().GetGenericTypeName();
		}
	}
}