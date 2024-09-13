using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

/// <summary>
///
/// </summary>
public class EnumerationModelBinderProvider
    : IModelBinderProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        return typeof(Enumeration).IsAssignableFrom(context.Metadata.ModelType) ? new EnumerationModelBinder() : null;
    }
}
