using System;
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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
                policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );
        });

        services.AddDbContextPool<OrderingContext>(configuration, (settings, provider, x) =>
        {
            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new InvalidOperationException(
                    "请通过 User Secrets 或 DbContexts__0__ConnectionString 环境变量配置数据库连接。不要提交数据库密码。");
            }

            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            x.UseLoggerFactory(loggerFactory);
            if ("mysql".Equals(settings.DatabaseType, StringComparison.OrdinalIgnoreCase))
            {
                // if (dbContextSettings.UseCompiledModel)
                // {
                //     x.LoadModel("Ordering.Infrastructure.CompileModels.OrderingContextModel, Ordering.Infrastructure");
                // }
                x.UseMySql(ServerVersion.AutoDetect(settings.ConnectionString), y =>
                {
                    y.Load(settings);
                });
            }
            else
            {
                x.UseNpgsql(y =>
                {
                    y.Load(settings);
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

        // Dapr Topic 端点保护（官方 App API Token 方案）：daprd 与应用容器均需配置
        // APP_API_TOKEN 环境变量（本地开发 dapr run 前 export APP_API_TOKEN=<token>）；
        // sidecar 回调必带 dapr-api-token 头，认证方案默认读取该变量进行校验
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.Audience = configuration["Authentication:Audience"];
                options.RequireHttpsMetadata = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters.ValidateAudience = true;
            })
            .AddDapr();
        services.AddAuthorization(options =>
        {
            options.AddDapr();
            options.AddPolicy("Ordering", policy => policy
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim("sub"));
        });
    });

    public static void Configure(this WebApplication app)
    {
        var env = app.Services.GetRequiredService<IHostEnvironment>();
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseCors("___my_cors");
        app.UseHealthChecks("/healthcheck");
        app.UseAuthentication();
        app.UseAuthorization();

        // dapr：Topic 端点安全由官方 App API Token 认证承担（见 ConfigureServices），
        // UseDaprSecurity 已废弃，不再使用
        app.UseCloudEvents();
        app.MapSubscribeHandler();
        app.MapDefaultControllerRoute().RequireCors("___my_cors");
        app.UseMicroserviceFramework();
        SeedData.Initialize(app.Services);

        var configuration = app.Configuration;
        var exit = configuration["exit"] == "true";
        if (exit)
        {
            app.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
        }
    }
}
