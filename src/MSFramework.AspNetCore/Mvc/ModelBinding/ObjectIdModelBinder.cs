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
    ///     从请求值中解析 ObjectId 并绑定到模型
    /// </summary>
    /// <param name="bindingContext">模型绑定上下文</param>
    /// <returns>异步任务</returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.FieldName).FirstValue;
        bindingContext.Result = !ObjectId.TryParse(value, out var id) && id != ObjectId.Empty
            ? ModelBindingResult.Failed()
            : ModelBindingResult.Success(id);

        return Task.CompletedTask;
    }
}
