using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MSFramework;
using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.SqlServer;
using MSFramework.EventBus;
using MSFramework.EventSouring;
using MSFramework.EventSouring.EntityFrameworkCore;
using Ordering.API.Application.EventHandler;
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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			Log.Logger.Information($"Use: Swagger");
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0", new OpenApiInfo {Version = "v1.0", Description = "西部证券研究发中心 API V1.0"});
			});
			services.AddMSFramework(builder =>
			{
				builder.Configuration = Configuration;
				builder.UseEntityFramework(ef => { ef.AddSqlServerDbContextOptionsBuilderCreator(); });
				builder.UseEntityFrameworkEventSouring();
  
				builder.AddLocalEventBus();

				builder.AddEventHandlers(typeof(UserCheckoutAcceptedEventHandler));
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			var es = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IEventStore>();
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
			//启用中间件服务生成Swagger作为JSON终结点
			app.UseSwagger();
			//启用中间件服务对swagger-ui，指定Swagger JSON终结点
			app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "西部证券研究发中心 API V1.0"); });
			app.UseMvc();
		}
	}
}