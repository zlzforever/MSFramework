using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using DotNetCore.CAP.Dapr;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.AspNetCore.Swagger;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.MySql;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Text.Json.Converters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RemoteConfiguration.Json.Aliyun;
using Serilog;
using Serilog.Events;
using Template.Domain;
using Template.Infrastructure;

namespace Template.API;

public static class Startup
{
	public static IConfigurationBuilder ConfigureConfiguration(this IConfigurationBuilder builder)
	{
		var configuration = builder.Build();

		var section = configuration.GetSection("Nacos");
		if (section.GetChildren().Any())
		{
			builder.AddNacosV2Configuration(section);
		}

		builder.AddAliyunJsonFile(source =>
		{
			source.Endpoint = configuration["RemoteConfiguration:Endpoint"];
			source.BucketName = configuration["RemoteConfiguration:BucketName"];
			source.AccessKeyId = configuration["RemoteConfiguration:AccessKeyId"];
			source.AccessKeySecret = configuration["RemoteConfiguration:AccessKeySecret"];
			source.Key = configuration["RemoteConfiguration:Key"];
		});

		return builder;
	}

	public static readonly Action<HostBuilderContext, IServiceCollection> ConfigureServices = (context, services) =>
	{
		var configuration = context.Configuration;

		ConfigureLogger(configuration);

		services.AddHttpContextAccessor();

		var jsonSerializerOptions = new JsonSerializerOptions();

		services.AddControllers(x =>
			{
				x.Filters.AddUnitOfWork()
					.AddAudit()
					.AddGlobalException();
				x.Filters.Add<SecurityDaprTopicFilter>();
				x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
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

		if (!context.HostingEnvironment.IsProduction())
		{
			services.AddSwaggerGen(x =>
			{
				x.SwaggerDoc("v1.0", new OpenApiInfo { Version = "v1.0", Description = "Template API V1.0" });
				x.CustomSchemaIds(type => type.FullName);
				x.MapEnumerationType(typeof(TemplateOptions).Assembly);
				x.SupportObjectId();
			});
		}

		services.AddHealthChecks();

		services.AddCap(x =>
		{
			x.UseEntityFramework<TemplateDbContext>();
			x.JsonSerializerOptions.AddDefaultConverters();

			x.UseDapr(configure => { configure.Pubsub = "pubsub"; });
			x.TopicNamePrefix = "Template";
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
			builder.UseAutoMapper();
			builder.UseMediator();
			builder.UseOptionsType(configuration);

			// 启用审计服务
			// builder.UseAuditStore<EfAuditStore<OrderingContext>>();
			builder.UseAspNetCore();
			// builder.UseNewtonsoftSerializer();

			builder.UseEntityFramework(x =>
			{
				// 添加 MySql 支持
				x.AddMySql<TemplateDbContext>(configuration);
			});
		});
	};

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
		app.UseDaprCap();

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

	private static void ConfigureLogger(IConfiguration configuration)
	{
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
	}
}