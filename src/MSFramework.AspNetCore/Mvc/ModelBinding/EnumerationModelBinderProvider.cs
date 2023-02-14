using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

public class EnumerationModelBinderProvider
    : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        return typeof(Enumeration).IsAssignableFrom(context.Metadata.ModelType) ? new EnumerationModelBinder() : null;
    }
}
