using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
// using MicroserviceFramework.AspNetCore.Swagger;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.MySql;
using MicroserviceFramework.Ef.PostgreSql;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.LocalEvent;
using MicroserviceFramework.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordering.Application;
using Ordering.Infrastructure;
using Serilog;
using Serilog.Events;

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
                .WriteTo.Async(x => x.File("auditing/log.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();
        }
    });

    public static readonly Action<HostBuilderContext, IServiceCollection> ConfigureServices = ((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddHttpClient();
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
            .AddDapr(x =>
            {
                x.UseHttpEndpoint("http://localhost:5101");
                x.UseGrpcEndpoint("http://localhost:5102");
            });
        services.AddHealthChecks();

        services.AddCors(option =>
        {
            option.AddPolicy("___my_cors", policy =>
                policy.AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    // .WithExposedHeaders("x-suggested-filename")
                    .AllowCredentials().SetPreflightMaxAge(TimeSpan.FromDays(30))
            );
        });

        var dbContextSettingsList = configuration.GetSection("DbContexts").Get<List<DbContextSettings>>();
        var dbContextSettings = dbContextSettingsList[0];

        services.AddDbContextPool<OrderingContext>((provider, x) =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            x.UseLoggerFactory(loggerFactory);
            if ("mysql".Equals(dbContextSettings.DatabaseType, StringComparison.OrdinalIgnoreCase))
            {
                // if (dbContextSettings.UseCompiledModel)
                // {
                //     x.LoadModel("Ordering.Infrastructure.CompileModels.OrderingContextModel, Ordering.Infrastructure");
                // }
                x.UseMySql(ServerVersion.AutoDetect(dbContextSettings.ConnectionString), y =>
                {
                    y.Load(dbContextSettings);
                });
            }
            else
            {
                x.UseNpgsql(y =>
                {
                    y.Load(dbContextSettings);
                });
            }
        });

        services.AddDbContext<TestDbContext>(x =>
            x.UseNpgsql());

        services.AddMicroserviceFramework(builder =>
        {
            builder.UseDependencyInjectionLoader();
            builder.UseOptionsType(configuration);
            builder.UseAutoMapperObjectAssembler();
            builder.UseEfAuditing<OrderingContext>();
            builder.UseLocalEventPublisher();
            builder.UseAspNetCoreExtension();
            builder.UseScopeServiceProvider();
            builder.UseEntityFramework();
        }, "Ordering");

        services.AddScoped<IDbContextFactory, EfDbContextFactory>();
    });

    public static void Configure(this WebApplication app)
    {
        var env = app.Services.GetRequiredService<IHostEnvironment>();
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

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

        var configuration = app.Configuration;
        var exit = configuration["exit"] == "true";
        if (exit)
        {
            app.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
        }
    }
}
