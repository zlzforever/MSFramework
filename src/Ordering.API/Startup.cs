using System.Text.Json;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.AspNetCore.Swagger;
using MicroserviceFramework.Audit;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Audit;
using MicroserviceFramework.Ef.PostgreSql;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Extensions.Options;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Text.Json.Converters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Ordering.Domain.AggregateRoots;
using Ordering.Infrastructure;

namespace Ordering.API;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions(Configuration);
        services.AddHttpContextAccessor();
        services.AddControllers(x =>
            {
                x.Filters.AddUnitOfWork()
                    .AddAudit()
                    .AddGlobalException()
                    .AddActionException();
                x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
            })
            .ConfigureInvalidModelStateResponse()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new ObjectIdJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            })
            // .AddNewtonsoftJson(x =>
            // {
            // 	x.SerializerSettings.Converters.Add(new ObjectIdConverter());
            // 	x.SerializerSettings.Converters.Add(new EnumerationConverter());
            // 	x.SerializerSettings.ContractResolver = new CompositeContractResolver
            // 	{
            // 		new EnumerationContractResolver(),
            // 		new CamelCasePropertyNamesContractResolver()
            // 	};
            // })
            .AddDapr();
        services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc("v1.0", new OpenApiInfo { Version = "v1.0", Description = "Ordering API V1.0" });
            x.CustomSchemaIds(type => type.FullName);
            x.MapEnumerationType(typeof(Address).Assembly);
            x.SupportObjectId();
        });
        services.AddHealthChecks();

        services.AddCap(x =>
        {
            x.UseEntityFramework<OrderingContext>();
            x.UseRedis("localhost");
        });

        services.AddMicroserviceFramework(builder =>
        {
            builder.UseAssemblyScanPrefix("Ordering");
            builder.UseDependencyInjectionLoader();
            builder.UseAutoMapper();
            builder.UseMediator();

            //builder.UseAccessControl(Configuration);
            // builder.UseRabbitMQEventDispatcher(new RabbitMQOptions(), typeof(UserCheckoutAcceptedEvent));
            // 启用审计服务
            builder.UseAuditStore<EfAuditStore>();
            // builder.UseMySqlMigrator(typeof(OrderingContext),
            // 	"Database='ordering';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306;");

            builder.UseAspNetCore();
            // builder.UseNewtonsoftSerializer();

            builder.UseEntityFramework(x =>
            {
                // 添加 MySql 支持
                x.AddNpgsql<OrderingContext, OrderingContext2>(Configuration);
            });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseHealthChecks("/healthcheck");

        app.UseAuthentication();
        app.UseAuthorization();

        //启用中间件服务生成Swagger作为JSON终结点
        app.UseSwagger();
        //启用中间件服务对swagger-ui，指定Swagger JSON终结点
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Ordering API V1.0"); });


        app.UseEndpoints(endpoints =>
        {
            // endpoints.MapSwagger();
            endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}").RequireCors("cors")
                ;
        });

        app.UseMicroserviceFramework();
    }
}
