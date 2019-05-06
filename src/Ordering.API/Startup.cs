using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.SqlServer;
using MSFramework.EventBus;
using MSFramework.EventSouring.EntityFrameworkCore;
using Ordering.API.Application.EventHandler;

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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0", new OpenApiInfo {Version = "v1.0", Description = "Ordering API V1.0"});
			});
			
			services.AddMSFramework(builder =>
			{
				builder.UseAspNetCoreSession();
				builder.UseEntityFramework(ef =>
				{
					// 添加 SqlServer 支持
					ef.AddSqlServerDbContextOptionsBuilderCreator();
				}, Configuration);
				builder.UseEntityFrameworkEventSouring();

				builder.UseLocalEventBus();

				builder.AddEventHandlers(typeof(UserCheckoutAcceptedEventHandler));
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

			app.UseMSFramework();
			app.UseHttpsRedirection();
			app.UseMvc();
			//启用中间件服务生成Swagger作为JSON终结点
			app.UseSwagger();
			//启用中间件服务对swagger-ui，指定Swagger JSON终结点
			app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Ordering API V1.0"); });
		}
	}
}