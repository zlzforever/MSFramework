using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroserviceFramework.Serialization.Newtonsoft
{
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
            if (settings == null)
            {
                settings = new JsonSerializerSettings();
                settings.Converters.Add(new ObjectIdConverter());
                settings.Converters.Add(new EnumerationConverter());
                settings.ContractResolver = new CompositeContractResolver
                {
                    new EnumerationContractResolver(),
                    new CamelCasePropertyNamesContractResolver()
                };
            }

            JsonConvert.DefaultSettings = () => settings;
            Default.JsonHelper = new NewtonsoftJsonHelper();

            return builder;
        }
    }
}