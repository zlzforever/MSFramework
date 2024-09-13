using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

/// <summary>
///
/// </summary>
public class ObjectIdModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(ObjectId) ? new ObjectIdModelBinder() : null;
    }
}
