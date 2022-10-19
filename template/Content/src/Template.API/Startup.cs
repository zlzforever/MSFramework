using System;
using System.Linq;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Template.Infrastructure;
using MicroserviceFramework.Text.Json.Converters;
using Template.Domain.Aggregates.Project;

namespace Template.API
{
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
			// 通过 Attribute 绑定 Options
			services.AddOptions(Configuration);
			// SwaggerUI require some service from views,
			// if your project a pure api, please use AddControllers
			services.AddControllers(x =>
				{
					// 使用过滤器 UnitOfWork，即在整个 Request 完成后提交 EF 的变更
					x.Filters.AddUnitOfWork();
					// 使用审计过滤器
					x.Filters.AddAudit();
					// 使用全局异常
					x.Filters.AddGlobalException();
					x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
				})
				.ConfigureInvalidModelStateResponse()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(new ObjectIdJsonConverter());
					options.JsonSerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());
					options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				});

			services.AddHealthChecks();
			services.AddSwaggerGen(x =>
			{
				x.SwaggerDoc("v1.0",
					new OpenApiInfo { Version = "v1.0", Description = "Template API V1.0" });
				x.CustomSchemaIds(type => type.FullName);
				x.MapEnumerationType(typeof(ProductType).Assembly);
				x.SupportObjectId();
			});

			services.AddResponseCompression(options =>
			{
				options.Providers.Add<BrotliCompressionProvider>();
				options.Providers.Add<GzipCompressionProvider>();
				options.MimeTypes =
					ResponseCompressionDefaults.MimeTypes.Concat(
						new[] { "image/svg+xml", "db" });
			});
			services.AddResponseCaching();

			services.AddRouting(x => { x.LowercaseUrls = true; });

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
				builder.UseAssemblyScanPrefix("Template");
				builder.UseDependencyInjectionLoader();
				builder.UseAutoMapper();
				builder.UseMediator();
				builder.UseAspNetCore();
				builder.UseAuditStore<EfAuditStore>();
				builder.UseEntityFramework(x => { x.AddNpgsql<TemplateDbContext>(Configuration); });
			});
			services.AddCap(x =>
			{
				x.UseEntityFramework<TemplateDbContext>();
			});

			services.ConfigureIdentityServer(Configuration);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				//启用中间件服务对swagger-ui，指定Swagger JSON终结点
				app.UseSwaggerUI(
					c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Template API V1.0"); });
			}
			// else
			// {
			// 	// app.UseExceptionHandler("/Home/Error");
			// 	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			// 	app.UseHsts();
			// }

			app.UseResponseCompression();
			app.UseResponseCaching();
			app.UseRouting();
			app.UseCors("cors");
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers()
					.RequireAuthorization("Jwt")
					.RequireCors("cors");
			});
			app.UseMicroserviceFramework();

			// 必须放在 UseMSFramework 后面
			var exit = Configuration["exit"] == "true";
			if (exit)
			{
				app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().StopApplication();
			}
		}
	}
}