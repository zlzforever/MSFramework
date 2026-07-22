using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft;

/// <summary>
///     Newtonsoft.Json 序列化的依赖注入扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     使用 Newtonsoft.Json 作为 JSON 序列化器，可传入自定义配置
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <param name="settings">JSON 序列化设置，为 null 时使用含 ObjectId/Enumeration 支持的默认配置</param>
    /// <returns>框架构建器</returns>
    public static MicroserviceFrameworkBuilder UseNewtonsoftJsonSerializer(this MicroserviceFrameworkBuilder builder,
        JsonSerializerSettings settings = null)
    {
        if (settings != null)
        {
            builder.Services.TryAddSingleton<IJsonSerializer>(new NewtonsoftJsonSerializer(settings));
        }
        else
        {
            var injectSettings = new JsonSerializerSettings();
            injectSettings.Converters.Add(new ObjectIdConverter());
            injectSettings.Converters.Add(new EnumerationConverter());
            injectSettings.ContractResolver = new CompositeContractResolver
            {
                new EnumerationContractResolver(), new CamelCasePropertyNamesContractResolver()
            };
            builder.Services.TryAddSingleton(injectSettings);
            builder.Services.TryAddSingleton<IJsonSerializer>(new NewtonsoftJsonSerializer(injectSettings));
        }

        return builder;
    }
}
