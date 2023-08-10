using System.Text.Json;
using MicroserviceFramework.Serialization;

namespace MicroserviceFramework.Text.Json;

public class DefaultJsonSerializerFactory : IJsonSerializerFactory
{
    private readonly JsonSerializerOptions _options;

    public DefaultJsonSerializerFactory(JsonSerializerOptions options)
    {
        _options = options;
    }

    public IJsonSerializer Create()
    {
        return new TextJsonSerializer(_options);
    }
}
