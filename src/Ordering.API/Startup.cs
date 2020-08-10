using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.AspNetCore.Extensions;
using MSFramework.AspNetCore.Filters;
using MSFramework.AspNetCore.Infrastructure;
using MSFramework.AspNetCore.AccessControl;
using MSFramework.Audits;
using MSFramework.AutoMapper;
using MSFramework.DependencyInjection;
using MSFramework.Ef;
using MSFramework.Ef.MySql;
using MSFramework.Extensions;
using MSFramework.Migrator.MySql;
using MSFramework.Shared;
using Newtonsoft.Json;
using Ordering.Domain;
using Ordering.Infrastructure;
using Serilog;

namespace Ordering.API
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
			
			 
			Configuration.Print(x => Log.Logger.Information(x));


			services.AddControllers(x =>
				{
					x.Filters.UseUnitOfWork();
					x.Filters.UseFunctionFilter();
					x.Filters.UseAudit();
					x.Filters.UseGlobalExceptionFilter();
					x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
				})
				.UseInvalidModelStateResponse()
				.AddNewtonsoftJson(x => { x.SerializerSettings.Converters.Add(new ObjectIdConverter()); });

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0", new OpenApiInfo {Version = "v1.0", Description = "Ordering API V1.0"});
				c.CustomSchemaIds(type => type.FullName);
				c.MapType<ObjectId>(() => new OpenApiSchema
				{
					Type = "string", Default = new OpenApiString(ObjectId.Empty.ToString()),
				});
			});
			services.AddHealthChecks();

			services.AddConfigType(typeof(AppOptions).Assembly);

			services.AddMSFramework(builder =>
			{
				builder.UseAutoMapper();
				builder.UseDependencyInjectionScanner();
				builder.UseEventDispatcher();
				builder.UseRequestProcessor();
				builder.UseNumberEncoding();
				builder.UseAccessControl(Configuration);
				// builder.UseRabbitMQEventDispatcher(new RabbitMQOptions(), typeof(UserCheckoutAcceptedEvent));
				// 启用审计服务
				builder.UseAudit();
				builder.UseMySqlMigrator(typeof(OrderingContext),
					"Database='ordering';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306;");

				builder.UseAspNetCore();
				// builder.AddPermission();

				builder.UseEntityFramework(x =>
				{
					// 添加 MySql 支持
					x.AddMySql<OrderingContext>(Configuration);
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseRouting();
			app.UseHealthChecks("/healthcheck");
			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
				endpoints.MapAreaControllerRoute("aa", "admin",
					"{area:exists}/{controller=Home}/{action=Index}/{id?}");
			});

			//启用中间件服务生成Swagger作为JSON终结点
			app.UseSwagger();
			//启用中间件服务对swagger-ui，指定Swagger JSON终结点
			app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Ordering API V1.0"); });

			app.UseMSFramework();
		}
	}
}