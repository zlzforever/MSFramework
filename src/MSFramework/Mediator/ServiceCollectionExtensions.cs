using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Mediator
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static MicroserviceFrameworkBuilder UseMediator(this MicroserviceFrameworkBuilder builder)
        {
            builder.Services.TryAddSingleton<IMediatorTypeMapper, MediatorTypeMapper>();
            builder.Services.TryAddScoped<IMediator, Mediator>();

            return builder;
        }
    }
}