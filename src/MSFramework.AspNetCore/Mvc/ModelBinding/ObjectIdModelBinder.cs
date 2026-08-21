using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding;

/// <summary>
///     MongoDB ObjectId 的模型绑定器，将字符串绑定为 ObjectId 类型
/// </summary>
public class ObjectIdModelBinder : IModelBinder
{
    /// <summary>
    ///     从请求值中解析 ObjectId 并绑定到模型，非法格式返回绑定失败（400）
    /// </summary>
    /// <param name="bindingContext">模型绑定上下文</param>
    /// <returns>异步任务</returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.FieldName).FirstValue;

        // 解析失败或解析结果为 Empty（非法输入）时写入 ModelState，
        // 让 ApiController 的模型验证流程能够返回真实的 HTTP 400。
        if (!ObjectId.TryParse(value, out var id) || id == ObjectId.Empty)
        {
            var key = string.IsNullOrEmpty(bindingContext.ModelName)
                ? bindingContext.FieldName
                : bindingContext.ModelName;
            bindingContext.ModelState.TryAddModelError(key,
                $"The value '{value}' is not valid.");
            bindingContext.Result = ModelBindingResult.Failed();
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Success(id);
        }

        return Task.CompletedTask;
    }
}
