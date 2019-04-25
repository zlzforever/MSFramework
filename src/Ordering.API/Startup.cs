using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore.SqlServer;
using MSFramework.EventBus;
using MSFramework.EventSouring;
using MSFramework.EventSouring.EntityFrameworkCore;
using Ordering.API.Application.EventHandler;
using Ordering.Domain;
using Ordering.Infrastructure;

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

			services.AddMSFramework(builder =>
			{
				builder.UseEntityFramework(ef =>
				{
					// 添加 SqlServer 支持
					ef.AddSqlServerDbContextOptionsBuilderCreator();
				}, Configuration);
				builder.UseEntityFrameworkEventSouring();

				builder.AddLocalEventBus();

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
		}
	}
}