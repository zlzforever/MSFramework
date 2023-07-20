using System.Text.Json;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.PostgreSql;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;

namespace MSFramework.AspNetCore.Test;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(x =>
            {
                x.Filters.AddUnitOfWork()
                    .AddAudit()
                    .AddGlobalException();
// #if !DEBUG
//                  x.Filters.Add<SecurityDaprTopicFilter>();
// #endif
                x.Filters.Add<ResponseWrapperFilter>();
                x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
                x.ModelBinderProviders.Insert(0, new EnumerationModelBinderProvider());
            })
            .ConfigureInvalidModelStateResponse()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.AddDefaultConverters();
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            });
        services.AddRouting(x => { x.LowercaseUrls = true; });
        services.AddMicroserviceFramework(builder =>
        {
            builder.UseAssemblyScanPrefix("Ordering");
            builder.UseDependencyInjectionLoader();
            builder.UseOptionsType(_configuration);
            builder.UseAutoMapper();
            // builder.UseMediator();
            builder.UseEventBus(options =>
            {
                options.AddAfterInterceptors(async provider =>
                {
                    await provider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
                });
            });
            builder.UseAspNetCore();
            builder.UseEntityFramework(x =>
            {
                //
                x.AddNpgsql<TestDataContext>(_configuration);
            });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();


        app.UseEndpoints(builder =>
        {
            builder.MapControllers();
        });

        app.UseMicroserviceFramework();
    }
}
