using System.Threading.Tasks;
using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Infrastructure
{
	public class ObjectIdModelBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var value = bindingContext.ValueProvider.GetValue(bindingContext.FieldName).FirstValue;
			bindingContext.Result = string.IsNullOrWhiteSpace(value)
				? ModelBindingResult.Failed()
				: ModelBindingResult.Success(new ObjectId(value));

			return Task.CompletedTask;
		}
	}
}