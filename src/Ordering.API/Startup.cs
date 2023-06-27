using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using DotNetCore.CAP.Dapr;
using DotNetCore.CAP.Messages;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Extensions;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                .WriteTo.Console().WriteTo.RollingFile(logFile)
                .CreateLogger();
        }
    });

    public static readonly Action<HostBuilderContext, IServiceCollection> ConfigureServices = ((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddHttpContextAccessor();
        var jsonSerializerOptions = new JsonSerializerOptions();
        // var settings = new JsonSerializerSettings();
        // settings.Converters.Add(new ObjectIdConverter());
        // settings.Converters.Add(new EnumerationConverter());
        // settings.ContractResolver = new CompositeContractResolver
        // {
        //     new EnumerationContractResolver(), new CamelCasePropertyNamesContractResolver()
        // };
        services.AddControllers(x =>
            {
                x.Filters.AddUnitOfWork()
                    .AddAudit()
                    .AddGlobalException();
#if !DEBUG
                 x.Filters.Add<SecurityDaprTopicFilter>();
#endif
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
            x.UseEntityFramework<OrderingContext>(y =>
            {
                y.Schema = "ordering";
            });
            x.JsonSerializerOptions.AddDefaultConverters();
            x.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            // var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
            // x.UseRabbitMQ(configure =>
            // {
            //     configure.HostName = "192.168.100.254";
            //     configure.Password = password;
            //     configure.UserName = "admin";
            //     // configure.ExchangeName = "ordering";
            // });
            x.UseDapr(y => y.Pubsub = "rabbitmq-pubsub");
            x.FailedRetryCount = 3;
            x.FailedMessageExpiredAfter = 365 * 24 * 3600;
            x.FailedThresholdCallback += failed =>
            {
                var traceId = failed.Message.Headers[Headers.MessageId];
                var messageBuilder = new StringBuilder($"消息名称: {failed.Message.GetName()}[{failed.MessageType}] ");
                messageBuilder.AppendLine();
                messageBuilder.AppendLine($"消息组: {failed.Message.GetGroup()}[CAP]");
                messageBuilder.AppendLine("消息ID(Content):");
                messageBuilder.AppendLine(failed.Message.GetId());
                messageBuilder.AppendLine($"日志跟踪标识: {traceId}");
                messageBuilder.AppendLine($"发送时间: {failed.Message.Headers[Headers.SentTime]}");
                messageBuilder.AppendLine("错误消息:");
                messageBuilder.AppendLine(failed.Message.Headers[Headers.Exception]);
                Log.Error(messageBuilder.ToString());
            };
            x.TopicNamePrefix = "CAP";
            x.UseDashboard();
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
            builder.UseAspNetCore();
            // builder.UseNewtonsoftJsonHelper(settings);

            builder.UseEntityFramework(x =>
            {
                // 添加 MySql 支持
                x.AddNpgsql<OrderingContext, AuditingContext>(configuration);
            });
        });
    });

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
        app.UseDaprCap();

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

        app.MapControllers();
        app.MapDefaultControllerRoute().RequireCors("cors");
        app.Use(async (context, next) =>
        {
            if (context == null)
            {
                return;
            }

            await next();


            // // 通常情况下异常会导致 Result 为空，但添加 ActionExceptionFilter 后，感知到导常后会返回 BadrequestObjectResult
            // // 是否有其它情况会导致 Result 为空?
            // if (actionExecutedContext.Result == null)
            // {
            //     return;
            // }
            //
            // actionExecutedContext.Result = actionExecutedContext.Result switch
            // {
            //     // 空内容是使用在 void/Task 这种 Action 中
            //     EmptyResult => new ObjectResult(ApiResult.Ok),
            //     _ => actionExecutedContext.Result
            // };
        });
        app.UseMicroserviceFramework();

        var configuration = app.Configuration;
        var exit = configuration["exit"] == "true";
        if (exit)
        {
            app.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
        }
    }
}
