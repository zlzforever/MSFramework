using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.AspNetCore.Swagger;
using MicroserviceFramework.Auditing.Loki;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.MySql;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RemoteConfiguration.Json.Aliyun;
using Serilog;
using Serilog.Events;
using Template.Domain;
using Template.Infrastructure;

namespace Template.API;

public static class StartupExtensions
{
    public static IHostBuilder ConfigureServices(this IHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            ConfigureLogger(context);

            var configuration = context.Configuration;
            services.AddHttpContextAccessor();

            services.AddControllers(x =>
                {
                    x.Filters.AddUnitOfWork();
                    x.Filters.AddAudit();
                    x.Filters.AddGlobalException();
                    x.Filters.AddResponseWrapper();
                    x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
                    x.ModelBinderProviders.Insert(0, new EnumerationModelBinderProvider());
                })
                .ConfigureInvalidModelStateResponse()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.AddDefaultConverters();
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
                .AddDapr(x =>
                {
                    // x.UseJsonSerializationOptions();
                    // 部署由命令控制
#if DEBUG
                    x.UseGrpcEndpoint("http://localhost:51001");
                    x.UseHttpEndpoint("http://localhost:51002");
#endif
                });

            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1.0", new OpenApiInfo { Version = "v1.0", Description = "Template API V1.0" });
                x.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
                x.MapEnumerationType(typeof(TemplateOptions).Assembly);
                x.SupportObjectId();
            });

            services.AddHealthChecks();

            services.AddCors(option =>
            {
                option.AddPolicy("___my_cors", policy =>
                    policy.AllowAnyMethod()
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        // 建议文件名应该在各 API 处理
                        // .WithExposedHeaders("x-suggested-filename")
                        .AllowCredentials().SetPreflightMaxAge(TimeSpan.FromDays(30))
                );
            });

            var dbContextSettingsList = configuration.GetSection("DbContexts").Get<List<DbContextSettings>>();
            var dbContextSettings = dbContextSettingsList[0];

            services.AddDbContext<TemplateDbContext>((provider, x) =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                x.UseLoggerFactory(loggerFactory);
                x.UseMySql(ServerVersion.AutoDetect(dbContextSettings.ConnectionString), y =>
                {
                    y.Load(dbContextSettings);
                    y.UseRemoveForeignKeyService();
                    y.UseRemoveExternalEntityService();
                });
            });

            services.AddMicroserviceFramework(x =>
            {
                x.UseAssemblyScanPrefix("Template");
                x.UseDependencyInjectionLoader();
                x.UseOptionsType(configuration);
                x.UseAutoMapperObjectAssembler();
                x.UseAspNetCore();
                x.UseEfAuditing<TemplateDbContext>();
                x.UseLokiAuditing();
                // builder.UseNewtonsoftSerializer();
                x.UseEntityFramework();
            });
        });
        builder.UseSerilog();

        return builder;
    }

    public static IHostBuilder ConfigureAppConfiguration(this IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(((context, configurationBuilder) =>
        {
            var configuration = context.Configuration;
            // comments by lewis 20230806
            // 不得再使用 nacos
            // var section = configuration.GetSection("Nacos");
            // if (section.GetChildren().Any())
            // {
            // 	builder.AddNacosV2Configuration(section);
            // }

            var section = configuration.GetSection("RemoteConfiguration");
            if (section.GetChildren().Any() &&
                !string.IsNullOrEmpty(configuration["RemoteConfiguration:Endpoint"]))
            {
                configurationBuilder.AddAliyunJsonFile(source =>
                {
                    source.Endpoint = configuration["RemoteConfiguration:Endpoint"];
                    source.BucketName = configuration["RemoteConfiguration:BucketName"];
                    source.AccessKeyId = configuration["RemoteConfiguration:AccessKeyId"];
                    source.AccessKeySecret = configuration["RemoteConfiguration:AccessKeySecret"];
                    source.Key = configuration["RemoteConfiguration:Key"];
                });
            }
        }));

        return builder;
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
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Template API V1.0"); });
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
        app.UseDaprSecurity();
        app.UseCloudEvents();
        app.MapSubscribeHandler();

        app.MapDefaultControllerRoute().RequireCors("___my_cors");

        app.UseMicroserviceFramework();

        if ("true".Equals(app.Configuration["exit"], StringComparison.OrdinalIgnoreCase))
        {
            app.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
        }
    }

    private static void ConfigureLogger(HostBuilderContext context)
    {
        var serilogSection = context.Configuration.GetSection("Serilog");
        if (serilogSection.GetChildren().Any())
        {
            Log.Logger = new LoggerConfiguration().ReadFrom
                .Configuration(context.Configuration)
                .CreateLogger();
        }
        else
        {
            var logFile = Environment.GetEnvironmentVariable("LOG_PATH");
            if (string.IsNullOrEmpty(logFile))
            {
                logFile = Environment.GetEnvironmentVariable("LOG");
            }

            if (string.IsNullOrEmpty(logFile))
            {
                logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/Template.log".ToLowerInvariant());
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
                .WriteTo.Console()
                .WriteTo.Async(x => x.File(logFile, rollingInterval: RollingInterval.Day))
                .CreateLogger();
        }
    }
}
