using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Infrastructure
{
	public class ObjectIdModelBinderProvider : IModelBinderProvider
	{
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			return context.Metadata.ModelType == typeof(ObjectId) ? new ObjectIdModelBinder() : null;
		}
	}
}