using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Enumeration 枚举类型模型绑定器提供程序
/// </summary>
public class EnumerationModelBinderProvider
    : IModelBinderProvider
{
    /// <summary>
    /// 获取 Enumeration 类型的模型绑定器
    /// </summary>
    /// <param name="context">模型绑定器提供程序上下文</param>
    /// <returns>Enumeration 模型绑定器实例</returns>
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        return typeof(Enumeration).IsAssignableFrom(context.Metadata.ModelType) ? new EnumerationModelBinder() : null;
    }
}
