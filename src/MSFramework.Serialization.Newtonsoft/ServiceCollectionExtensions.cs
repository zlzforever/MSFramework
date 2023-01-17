using Newtonsoft.Json;

namespace MicroserviceFramework.Serialization.Newtonsoft;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseNewtonsoftJsonHelper(this MicroserviceFrameworkBuilder builder,
        JsonSerializerSettings settings = null)
    {
        Defaults.JsonHelper = new NewtonsoftJsonHelper(settings);

        return builder;
    }
}
