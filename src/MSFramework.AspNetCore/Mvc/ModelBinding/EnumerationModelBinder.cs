using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

/// <summary>
///     Enumeration 类型的模型绑定器，将字符串绑定为 Enumeration 子类实例
/// </summary>
public class EnumerationModelBinder : IModelBinder
{
    /// <summary>
    ///     从请求值中解析 Enumeration 并绑定到模型
    /// </summary>
    /// <param name="bindingContext">模型绑定上下文</param>
    /// <returns>异步任务</returns>
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
