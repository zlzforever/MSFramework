using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.AspNetCore.Swagger;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Auditing;
using MicroserviceFramework.Ef.MySql;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RemoteConfiguration.Json.Aliyun;
using Serilog;
using Serilog.Events;
using Template.Domain;
using Template.Infrastructure;

namespace Template.API;

public static class StartupExtensions
{
	public static WebApplicationBuilder ConfigureService(this WebApplicationBuilder builder)
	{
		ConfigureLogger(builder);

		builder.Services.AddHttpContextAccessor();

		var jsonSerializerOptions = new JsonSerializerOptions();

		builder.Services.AddControllers(x =>
			{
				x.Filters.AddUnitOfWork()
					.AddAudit()
					.AddGlobalException();
				x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
				x.ModelBinderProviders.Insert(0, new EnumerationModelBinderProvider());
			})
			.ConfigureInvalidModelStateResponse()
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.AddDefaultConverters();
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				jsonSerializerOptions = options.JsonSerializerOptions;
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
				x.UseJsonSerializationOptions(jsonSerializerOptions);
				// 部署由命令控制
#if DEBUG
				x.UseGrpcEndpoint("http://localhost:51001");
				x.UseHttpEndpoint("http://localhost:51002");
#endif
			});

		builder.Services.AddSwaggerGen(x =>
		{
			x.SwaggerDoc("v1.0", new OpenApiInfo { Version = "v1.0", Description = "Template API V1.0" });
			x.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
			x.MapEnumerationType(typeof(TemplateOptions).Assembly);
			x.SupportObjectId();
		});

		builder.Services.AddHealthChecks();

		// comments: 使用 dapr 的 pubsub 稳定性有一定保证
		// builder.Services.AddCap(x =>
		// {
		// 	x.UseEntityFramework<TemplateDbContext>(y =>
		// 	{
		// 		y.Schema = "ordering";
		// 	});
		// 	x.JsonSerializerOptions.AddDefaultConverters();
		// 	x.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
		// 	// var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
		// 	// x.UseRabbitMQ(configure =>
		// 	// {
		// 	//     configure.HostName = "192.168.100.254";
		// 	//     configure.Password = password;
		// 	//     configure.UserName = "admin";
		// 	//     // configure.ExchangeName = "ordering";
		// 	// });
		// 	x.UseDapr(y => y.Pubsub = "rabbitmq-pubsub");
		// 	x.FailedRetryCount = 3;
		// 	x.FailedMessageExpiredAfter = 365 * 24 * 3600;
		// 	x.FailedThresholdCallback += failed =>
		// 	{
		// 		var traceId = failed.Message.Headers[Headers.MessageId];
		// 		var messageBuilder = new StringBuilder($"消息名称: {failed.Message.GetName()}[{failed.MessageType}] ");
		// 		messageBuilder.AppendLine();
		// 		messageBuilder.AppendLine($"消息组: {failed.Message.GetGroup()}[CAP]");
		// 		messageBuilder.AppendLine("消息ID(Content):");
		// 		messageBuilder.AppendLine(failed.Message.GetId());
		// 		messageBuilder.AppendLine($"日志跟踪标识: {traceId}");
		// 		messageBuilder.AppendLine($"发送时间: {failed.Message.Headers[Headers.SentTime]}");
		// 		messageBuilder.AppendLine("错误消息:");
		// 		messageBuilder.AppendLine(failed.Message.Headers[Headers.Exception]);
		// 		Log.Error(messageBuilder.ToString());
		// 	};
		// 	x.TopicNamePrefix = "CAP";
		// 	x.UseDashboard();
		// });

		builder.Services.AddCors(option =>
		{
			option
				.AddPolicy("cors", policy =>
					policy.AllowAnyMethod()
						.SetIsOriginAllowed(_ => true)
						.AllowAnyHeader()
						// 建议文件名应该在各 API 处理
						// .WithExposedHeaders("x-suggested-filename")
						.AllowCredentials().SetPreflightMaxAge(TimeSpan.FromDays(30))
				);
		});

		builder.Services.AddMicroserviceFramework(x =>
		{
			x.UseAssemblyScanPrefix("Template");
			x.UseDependencyInjectionLoader();
			x.UseAutoMapper();
			x.UseOptionsType(builder.Configuration);
			x.UseAspNetCore();
			x.UseAuditingStore<LokiAuditingStore>();
			x.UseAuditingStore<EfAuditingStore>();
			x.UseEventBus((_, options) =>
			{
				options.AddAfterInterceptor(async (provider, _) =>
				{
					await provider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
				});
			});
			// builder.UseNewtonsoftSerializer();

			x.UseEntityFramework(e =>
			{
				// 添加 MySql 支持
				e.AddMySql<TemplateDbContext>(builder.Configuration);
			});
		});

		return builder;
	}

	public static WebApplicationBuilder ConfigureConfiguration(this WebApplicationBuilder builder)
	{
		// comments by lewis 20230806
		// 不得再使用 nacos
		// var section = configuration.GetSection("Nacos");
		// if (section.GetChildren().Any())
		// {
		// 	builder.AddNacosV2Configuration(section);
		// }

		var section = builder.Configuration.GetSection("RemoteConfiguration");
		if (section.GetChildren().Any() && !string.IsNullOrEmpty(builder.Configuration["RemoteConfiguration:Endpoint"]))
		{
			builder.Configuration.AddAliyunJsonFile(source =>
			{
				source.Endpoint = builder.Configuration["RemoteConfiguration:Endpoint"];
				source.BucketName = builder.Configuration["RemoteConfiguration:BucketName"];
				source.AccessKeyId = builder.Configuration["RemoteConfiguration:AccessKeyId"];
				source.AccessKeySecret = builder.Configuration["RemoteConfiguration:AccessKeySecret"];
				source.Key = builder.Configuration["RemoteConfiguration:Key"];
			});
		}

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
		app.UseCloudEvents();
		app.MapSubscribeHandler();

		app.MapControllers();
		app.MapDefaultControllerRoute().RequireCors("cors");

		app.UseMicroserviceFramework();

		var configuration = app.Configuration;
		var exit = configuration["exit"] == "true";
		if (exit)
		{
			app.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
		}
	}

	private static void ConfigureLogger(WebApplicationBuilder builder)
	{
		var serilogSection = builder.Configuration.GetSection("Serilog");
		if (serilogSection.GetChildren().Any())
		{
			Log.Logger = new LoggerConfiguration().ReadFrom
				.Configuration(builder.Configuration)
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
				.WriteTo.Console().WriteTo.RollingFile(logFile)
				.CreateLogger();
		}

		builder.Host.UseSerilog();
	}
}