using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

/// <summary>
///
/// </summary>
public class EnumerationModelBinder : IModelBinder
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.FieldName).FirstValue;
        if (string.IsNullOrWhiteSpace(value))
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }
        else
        {
            var result = Enumeration.Parse(bindingContext.ModelType, value);
            bindingContext.Result = ModelBindingResult.Success(result);
        }

        return Task.CompletedTask;
    }
}
