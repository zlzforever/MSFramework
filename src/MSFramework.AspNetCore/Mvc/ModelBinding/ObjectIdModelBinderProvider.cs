using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// ObjectId 模型绑定器提供程序，用于将请求参数绑定到 MongoDB.ObjectId 类型
/// </summary>
public class ObjectIdModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// 获取 ObjectId 类型的模型绑定器
    /// </summary>
    /// <param name="context">模型绑定器提供程序上下文</param>
    /// <returns>ObjectId 模型绑定器实例</returns>
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(ObjectId) ? new ObjectIdModelBinder() : null;
    }
}
