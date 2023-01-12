using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using DotNetCore.CAP.Dapr;
using DotNetCore.CAP.Internal;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.AspNetCore.Swagger;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.PostgreSql;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Text.Json.Converters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Ordering.Domain.AggregateRoots;
using Ordering.Infrastructure;
using MicroserviceFramework.Extensions.Options;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Serialization.Newtonsoft;
using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ordering.Application.Events;
using Serilog;
using Serilog.Events;
using Defaults = MicroserviceFramework.Defaults;

namespace Ordering.API;

public static class Startup
{
    public static readonly Action<IConfigurationBuilder> ConfigureConfiguration = (builder =>
    {
        var configuration = builder.Build();

        var serilogSection = configuration.GetSection("Serilog");
        if (serilogSection.GetChildren().Any())
        {
            Log.Logger = new LoggerConfiguration().ReadFrom
                .Configuration(configuration)
                .CreateLogger();
        }
        else
        {
            var logFile = Environment.GetEnvironmentVariable("LOG");
            if (string.IsNullOrEmpty(logFile))
            {
                logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/ordering.log");
            }

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console().WriteTo.RollingFile(logFile)
                .CreateLogger();
        }
    });

    public static readonly Action<HostBuilderContext, IServiceCollection> ConfigureServices = ((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddHttpContextAccessor();
        var jsonSerializerOptions = new JsonSerializerOptions();
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new ObjectIdConverter());
        settings.Converters.Add(new EnumerationConverter());
        settings.ContractResolver = new CompositeContractResolver
        {
            new EnumerationContractResolver(), new CamelCasePropertyNamesContractResolver()
        };

        services.AddControllers(x =>
            {
                x.Filters.AddUnitOfWork()
                    .AddAudit()
                    .AddGlobalException().AddActionException();
#if !DEBUG
                 x.Filters.Add<SecurityDaprTopicFilter>();
#endif
                x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
                x.ModelBinderProviders.Insert(0, new EnumerationModelBinderProvider());
            })
            .ConfigureInvalidModelStateResponse()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new ObjectIdJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());
                options.JsonSerializerOptions.Converters.Add(new PagedResultJsonConverterFactory());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                jsonSerializerOptions = options.JsonSerializerOptions;
            })
            // .AddNewtonsoftJson(x =>
            // {
            //     x.SerializerSettings.Converters.Add(new ObjectIdConverter());
            //     x.SerializerSettings.Converters.Add(new EnumerationConverter());
            //     // x.SerializerSettings.ContractResolver = new CompositeContractResolver
            //     // {
            //     //     new EnumerationContractResolver(), new CamelCasePropertyNamesContractResolver()
            //     // };
            //     settings = x.SerializerSettings;
            // })
            .AddDapr(x =>
            {
                x.UseJsonSerializationOptions(jsonSerializerOptions);
#if DEBUG
                x.UseGrpcEndpoint("http://localhost:51001");
                x.UseHttpEndpoint("http://localhost:50001");
#endif
            });
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
            x.JsonSerializerOptions.Converters.Add(new ObjectIdJsonConverter());
            x.JsonSerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());


//            x.DefaultGroupName = "pubsub";
            x.UseDapr(configure =>
            {
                configure.Pubsub = "pubsub";
            });
            x.TopicNamePrefix = "CAP";
            x.UseDashboard();
            x.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        });

        services.AddCors(option =>
        {
            option
                .AddPolicy("cors", policy =>
                    policy.AllowAnyMethod()
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .WithExposedHeaders("x-suggested-filename")
                        .AllowCredentials().SetPreflightMaxAge(TimeSpan.FromDays(30))
                );
        });

        services.AddMicroserviceFramework(builder =>
        {
            builder.UseAssemblyScanPrefix("Ordering");
            builder.UseDependencyInjectionLoader();
            builder.UseOptionsType(configuration);
            builder.UseAutoMapper();
            builder.UseMediator();
            builder.UseEventBus(options =>
            {
                options.AddAfterInterceptors(async provider =>
                {
                    await provider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
                });
            });
            // 启用审计服务
            // builder.UseAuditStore<EfAuditStore<OrderingContext>>();
            builder.UseAspNetCore();
            builder.UseNewtonsoftJsonHelper(settings);

            builder.UseEntityFramework(x =>
            {
                // 添加 MySql 支持
                x.AddNpgsql<OrderingContext, OrderingContext2>(configuration);
            });
        });
    });

    public class A
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
    }

    public static void Configure(this WebApplication app)
    {
        var env = app.Services.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Ordering API V1.0"); });
        }
        // else
        // {
        //     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        //     app.UseHsts();
        // }

        app.UseRouting();
        app.UseHealthChecks("/healthcheck");
        app.UseAuthentication();
        app.UseAuthorization();

        // dapr
        app.UseCloudEvents();
        app.MapSubscribeHandler();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute().RequireCors("cors");
            endpoints.MapControllers();
        });
        app.UseDaprCap();

        app.UseMicroserviceFramework();
    }
}
