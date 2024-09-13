using System;
using System.Linq;

namespace MicroserviceFramework.Runtime;

/// <summary>
///
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <param name="excludeInterfaces"></param>
    /// <returns></returns>
    public static Type[] GetInterfacesExcludeBy(this Type type, params Type[] excludeInterfaces)
    {
        var types = type.GetInterfaces();
        if (excludeInterfaces is { Length: > 0 })
        {
            types = types.Where(t => !excludeInterfaces.Contains(t)).ToArray();
        }

        for (var index = 0; index < types.Length; index++)
        {
            var interfaceType = types[index];
            if (interfaceType.IsGenericType && !interfaceType.IsGenericTypeDefinition &&
                interfaceType.FullName == null)
            {
                types[index] = interfaceType.GetGenericTypeDefinition();
            }
        }

        return types;
    }

    // public static void SetProperty(this object obj, string propertyName, dynamic value)
    // {
    //     if (obj == null)
    //     {
    //         return;
    //     }
    //
    //     var property = obj.GetType().GetProperty(propertyName);
    //     if (property == null)
    //     {
    //         return;
    //     }
    //
    //     if (property.CanWrite)
    //     {
    //         property.SetValue(obj, value);
    //     }
    //     else
    //     {
    //         throw new NotSupportedException($"{propertyName} 没有 setter");
    //     }
    // }
}
