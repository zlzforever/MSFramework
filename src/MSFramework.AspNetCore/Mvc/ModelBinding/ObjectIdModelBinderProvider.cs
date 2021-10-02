using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding
{
	public class ObjectIdModelBinderProvider : IModelBinderProvider
	{
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			return context.Metadata.ModelType == typeof(ObjectId) ? new ObjectIdModelBinder() : null;
		}
	}
}