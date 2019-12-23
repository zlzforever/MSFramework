using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.Common;
using MSFramework.Ef;
using MSFramework.Ef.MySql;
using MSFramework.Ef.SqlServer;
using MSFramework.EventBus;
using MSFramework.Permission;
using Ordering.Application.Event;

namespace Ordering.API
{
	class MyInitializer : Initializer
	{
		public override void Initialize(IApplicationBuilder builder)
		{
		}
	}

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
			services.AddControllersWithViews();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0", new OpenApiInfo {Version = "v1.0", Description = "Ordering API V1.0"});
			});
			services.AddHealthChecks();

			services.AddMSFramework(builder =>
			{
				builder.AddEventHandler(typeof(UserCheckoutAcceptedEvent));
				// 开发环境可以使用本地消息总线，生产环境应该换成分布式消息队列
				builder.AddPassThroughEventBus();
				builder.AddAspNetCoreSession();
				builder.AddInitializer<MyInitializer>();
				builder.AddPermission();
				builder.AddEntityFramework(x =>
				{
					// 添加 SqlServer 支持
					x.AddSqlServerDbContextOptionsBuilderCreator();
					// 添加 MySql 支持
					x.AddMySqlDbContextOptionsBuilderCreator();
				}, Configuration);
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
			app.UseMSFramework();
			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});

			app.Use(async (context, next) =>
			{
				if (context.Request.Path == "/")
				{
					context.Response.Redirect("swagger");
				}
				else
				{
					await next();
				}
			});
			//启用中间件服务生成Swagger作为JSON终结点
			app.UseSwagger();
			//启用中间件服务对swagger-ui，指定Swagger JSON终结点
			app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Ordering API V1.0"); });
		}
	}
}