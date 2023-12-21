using System;
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
using MicroserviceFramework.Ef.PostgreSql;
using MicroserviceFramework.Ef.SqlServer;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Ordering.Domain.AggregateRoots;
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
                .WriteTo.Console()
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
                x.UseHttpEndpoint("http://localhost:5101");
                x.UseGrpcEndpoint("http://localhost:5102");
            });
        services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc("v1.0", new OpenApiInfo { Version = "v1.0", Description = "Ordering API V1.0" });
            x.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
            x.MapEnumerationType(typeof(Address).Assembly);
            x.SupportObjectId();
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

        services.AddDbContext<OrderingContext>((provider, x) =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            x.UseLoggerFactory(loggerFactory);
            x.UseNpgsql(y =>
            {
                y.LoadFromConfiguration(provider);
                y.UseRemoveForeignKeyService();
                y.UseRemoveExternalEntityService();
            });
            // x.UseMySql(provider, y =>
            // {
            //     y.LoadFromConfiguration(provider);
            //     y.UseRemoveForeignKeyService();
            //     y.UseRemoveExternalEntityService();
            // });
            // x.UseSqlServer(y =>
            // {
            //     y.LoadFromConfiguration(provider);
            //     y.UseRemoveForeignKeyService();
            //     y.UseRemoveExternalEntityService();
            // });
        });

        // services.AddAssemblyScanPrefix("Ordering");
        // services.AddDependencyInjectionLoader();
        // services.AddOptionsType(configuration);
        // services.AddEfAuditing<OrderingContext>();
        // services.AddLocalEventPublisher();
        // services.AddLokiAuditing();
        // services.AddAspNetCore();

        services.AddMicroserviceFramework(builder =>
        {
            builder.UseAssemblyScanPrefix("Ordering");
            builder.UseDependencyInjectionLoader();
            builder.UseOptionsType(configuration);
            builder.UseAutoMapperObjectAssembler();
            builder.UseEfAuditing<OrderingContext>();
            builder.UseLokiAuditing();
            builder.UseLocalEventPublisher();
            builder.UseAspNetCore();
            // builder.UseNewtonsoftJsonHelper(settings);
            builder.UseEntityFramework();
        });
    });

    public static void Configure(this WebApplication app)
    {
        var b = app.Services.CreateScope().ServiceProvider.GetRequiredService<IOptions<DbContextSettingsList>>().Value;
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
        app.UseDaprSecurity();
        app.UseCloudEvents();
        app.MapSubscribeHandler();

        // app.UseDaprCap();

        // app.Use(async (context, next) =>
        // {
        //     var capPublisher = context.RequestServices.GetService<ICapPublisher>();
        //     if (capPublisher == null)
        //     {
        //         await next();
        //     }
        //     else
        //     {
        //         var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        //             .CreateLogger("CAP.TransactionFilter");
        //
        //         var dbContext = context.RequestServices.GetRequiredService<OrderingContext>();
        //         logger.LogDebug("开启 CAP EF 事务");
        //
        //         await using var transaction = dbContext.Database.BeginTransaction(capPublisher);
        //
        //         await next.Invoke();
        //
        //         // 200 说明执行成功， 没有异常
        //         if (context.Response.StatusCode == 200)
        //         {
        //             await transaction.CommitAsync();
        //             logger.LogDebug("提交 CAP EF 事务成功");
        //         }
        //         else
        //         {
        //             await transaction.RollbackAsync();
        //             logger.LogDebug("回滚 CAP EF 事务成功");
        //         }
        //     }
        // });

        // 中间件顺序
        // ExceptionHandler
        // HSTS
        // HttpsRedirection
        // StaticFiles
        // Routing
        // Cors
        // Authentication
        // Authorization
        // CustomMiddleware
        // EndpointRouting

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
