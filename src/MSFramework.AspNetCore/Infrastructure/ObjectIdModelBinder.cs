using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MSFramework.Common;

namespace MSFramework.AspNetCore.Infrastructure
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