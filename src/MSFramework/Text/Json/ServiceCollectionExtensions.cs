using System.Text.Json;
using MicroserviceFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Text.Json;

/// <summary>
/// System.Text.Json 序列化注册扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 使用 System.Text.Json 作为默认 JSON 序列化器
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <param name="options">JSON 序列化选项，为 null 时使用默认配置</param>
    /// <returns>框架构建器</returns>
    public static MicroserviceFrameworkBuilder UseTextJsonSerializer(this MicroserviceFrameworkBuilder builder,
        JsonSerializerOptions options = null)
    {
        if (options != null)
        {
            builder.Services.TryAddSingleton<IJsonSerializer>(new TextJsonSerializer(options));
        }
        else
        {
            builder.Services.TryAddSingleton(provider =>
            {
                var x = provider.GetService<JsonSerializerOptions>();
                return x != null ? new TextJsonSerializer(x) : TextJsonSerializer.Create();
            });
        }

        return builder;
    }
}
