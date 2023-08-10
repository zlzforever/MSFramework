using Newtonsoft.Json;

namespace MicroserviceFramework.Serialization.Newtonsoft;

public class DefaultJsonSerializerFactory : IJsonSerializerFactory
{
    private readonly JsonSerializerSettings _options;

    public DefaultJsonSerializerFactory(JsonSerializerSettings options)
    {
        _options = options;
    }

    public IJsonSerializer Create()
    {
        return new NewtonsoftJsonSerializer(_options);
    }
}
