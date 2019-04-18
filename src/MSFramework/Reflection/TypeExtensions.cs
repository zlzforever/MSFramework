using System;
using System.Linq;

namespace MSFramework.Reflection
{
	public static class TypeExtensions
	{
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

		public static Type[] GetImplementedInterfaces(this Type type, params Type[] exceptInterfaces)
		{
			Type[] interfaceTypes = type.GetInterfaces().Where(t => !exceptInterfaces.Contains(t)).ToArray();
			for (int index = 0; index < interfaceTypes.Length; index++)
			{
				Type interfaceType = interfaceTypes[index];
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