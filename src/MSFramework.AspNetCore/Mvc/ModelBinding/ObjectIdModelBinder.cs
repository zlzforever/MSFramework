using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding
{
	public class ObjectIdModelBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var value = bindingContext.ValueProvider.GetValue(bindingContext.FieldName).FirstValue;
			bindingContext.Result = !ObjectId.TryParse(value, out var id) && id != ObjectId.Empty
				? ModelBindingResult.Failed()
				: ModelBindingResult.Success(id);

			return Task.CompletedTask;
		}
	}
}