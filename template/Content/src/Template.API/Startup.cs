using System;
using System.Linq;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.Audit;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Audit;
using MicroserviceFramework.Ef.PostgreSql;
using MicroserviceFramework.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Template.Infrastructure;
using MicroserviceFramework.Serialization.Converters;
using Microsoft.EntityFrameworkCore;

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
			// SwaggerUI require some service from views,
			// if your project a pure api, please use AddControllers
			services.AddControllers(x =>
				{
					x.Filters.AddUnitOfWork();
					x.Filters.AddAudit();
					x.Filters.AddGlobalException();
					x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
				})
				.ConfigureInvalidModelStateResponse()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(new ObjectIdJsonConverter());
					options.JsonSerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());
					options.JsonSerializerOptions.Converters.Add(new EnumerationJsonConverter());
				});

			services.AddHealthChecks();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0",
					new OpenApiInfo { Version = "v1.0", Description = "Template API V1.0" });
				c.CustomSchemaIds(x => x.FullName);
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


			services.AddOptions(Configuration);
			services.AddMicroserviceFramework(builder =>
			{
				// builder.UseNewtonsoftJson();
				builder.UseAutoMapper();
				builder.UseAspNetCore();
				builder.UseAuditStore<EfAuditStore>();
				builder.UseAssemblyScanPrefix("Template");
				builder.UseEntityFramework(x => { x.AddNpgsql<TemplateDbContext>(Configuration); });
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
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

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