using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using MongoDB.Bson;
using Xunit;

namespace MSFramework.Tests;

public class ObjectIdModelBinderTests
{
    [Fact]
    public async Task BindModelAsync_ReturnsFailed_ForInvalidValue()
    {
        var context = CreateContext("invalid-object-id");

        await new ObjectIdModelBinder().BindModelAsync(context);

        Assert.Equal(ModelBindingResult.Failed(), context.Result);
    }

    [Fact]
    public async Task BindModelAsync_ReturnsFailed_ForEmptyObjectId()
    {
        // 旧实现中 !TryParse && id == Empty 恒为 false，非法输入被静默绑定为 Empty；新实现必须绑定失败
        var context = CreateContext(ObjectId.Empty.ToString());

        await new ObjectIdModelBinder().BindModelAsync(context);

        Assert.Equal(ModelBindingResult.Failed(), context.Result);
    }

    [Fact]
    public async Task BindModelAsync_ReturnsSuccess_ForValidValue()
    {
        var id = ObjectId.GenerateNewId();
        var context = CreateContext(id.ToString());

        await new ObjectIdModelBinder().BindModelAsync(context);

        Assert.True(context.Result.IsModelSet);
        Assert.Equal(id, (ObjectId)context.Result.Model);
    }

    /// <summary>
    /// 构建指定字段值的默认模型绑定上下文
    /// </summary>
    /// <param name="value">字段值</param>
    /// <returns>模型绑定上下文</returns>
    private static DefaultModelBindingContext CreateContext(string value)
    {
        return new DefaultModelBindingContext
        {
            ModelName = "id",
            FieldName = "id",
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(ObjectId)),
            ValueProvider = new TestValueProvider(value),
            ActionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            ModelState = new ModelStateDictionary()
        };
    }

    /// <summary>
    /// 始终返回固定值的值提供器
    /// </summary>
    private sealed class TestValueProvider(string value) : IValueProvider
    {
        public bool ContainsPrefix(string prefix)
        {
            return true;
        }

        public ValueProviderResult GetValue(string key)
        {
            return new ValueProviderResult(value);
        }
    }
}
