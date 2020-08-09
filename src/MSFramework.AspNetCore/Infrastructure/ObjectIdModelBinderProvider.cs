using Microsoft.AspNetCore.Mvc.ModelBinding;
using MSFramework.Shared;

namespace MSFramework.AspNetCore.Infrastructure
{
	public class ObjectIdModelBinderProvider : IModelBinderProvider
	{
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			return context.Metadata.ModelType == typeof(ObjectId) ? new ObjectIdModelBinder() : null;
		}
	}
}