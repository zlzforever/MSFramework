using System;
using System.Linq;

namespace MicroserviceFramework.Runtime
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

        public static void SetProperty(this object obj, string propertyName, dynamic value)
        {
            if (obj == null)
            {
                return;
            }

            var property = obj.GetType().GetProperty(propertyName);
            if (property == null)
            {
                return;
            }

            if (property.CanWrite)
            {
                property.SetValue(obj, value);
            }
            else
            {
                throw new NotSupportedException($"{propertyName} 没有 setter");
            }
        }
    }
}