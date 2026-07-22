using System;
using System.Linq;

namespace MicroserviceFramework.Runtime;

/// <summary>
/// <see cref="Type"/> 扩展方法
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// 获取类型实现的接口，排除指定的接口并泛型类型规范化。
    /// </summary>
    /// <param name="type">目标类型</param>
    /// <param name="excludeInterfaces">要排除的接口类型</param>
    /// <returns>过滤后的接口类型数组</returns>
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
