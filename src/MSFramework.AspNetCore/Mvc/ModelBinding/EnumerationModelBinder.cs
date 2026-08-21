using System;
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
    ///     从请求值中解析 Enumeration 并绑定到模型。
    ///     非法枚举值归入模型状态错误（由 <c>InvalidModelStateResponseFactory</c> 统一响应），
    ///     而不是抛未处理异常导致 500
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
            try
            {
                var result = Enumeration.Parse(bindingContext.ModelType, value);
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            catch (InvalidOperationException)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                    $"枚举值 '{value}' 无效");
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }

        return Task.CompletedTask;
    }
}
