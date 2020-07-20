using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.AspNetCore.Filters;
using MSFramework.Audit;
using MSFramework.AutoMapper;
using MSFramework.DependencyInjection;
using MSFramework.Ef;
using MSFramework.Ef.MySql;
using MSFramework.Extensions;
using MSFramework.Migrator.MySql;
using Serilog;
using Template.Domain;
using Template.Infrastructure;

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
			Configuration.Print(x => Log.Logger.Information(x));

			// SwaggerUI require some service from views,
			// if your project a pure api, please use AddControllers
			services.AddControllers(x =>
				{
					x.Filters.UseUnitOfWork();
					x.Filters.UseFunctionFilter();
					x.Filters.UseAudit();
					x.Filters.UseGlobalExceptionFilter();
					x.Filters.UseInvalidModelStateFilter();
				})
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
				.AddNewtonsoftJson();

			services.AddHealthChecks();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0",
					new OpenApiInfo {Version = "v1.0", Description = "Template API V1.0"});
				c.CustomSchemaIds(x => x.FullName);
			});

			services.AddResponseCompression();
			services.AddResponseCaching();

			services.AddRouting(x => { x.LowercaseUrls = true; });

#if !DEBUG
			var options = new AppOptions(Configuration);
			services.AddAuthentication("Bearer")
				.AddIdentityServerAuthentication(x =>
				{
					x.Authority = options.Authority;
					x.RequireHttpsMetadata = options.RequireHttpsMetadata;
					x.ApiName = options.ApiName;
					if (!string.IsNullOrWhiteSpace(options.ApiSecret))
					{
						x.ApiSecret = options.ApiSecret;
					}
				});
#endif

			services.AddScoped<AppOptions>();
			services.AddMSFramework(builder =>
			{
				builder.UseAutoMapper();
				builder.UseDependencyInjectionScanner();
				builder.UseEventDispatcher();
				// builder.UseRabbitMQEventDispatcher(new RabbitMQOptions(), typeof(UserCheckoutAcceptedEvent));
				// 启用审计服务
				builder.UseAudit();
				builder.UseMySqlMigrator(typeof(AppDbContext),
					"Database='template';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306;");

				builder.UseAspNetCore();
				builder.UseEntityFramework(x =>
				{
					// 添加 MySql 支持
					x.AddMySql<AppDbContext>(Configuration);
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			// 必须放在 UseMSFramework 后面
			var exit = Configuration["exit"] == "true";
			if (exit)
			{
				app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().StopApplication();
				return;
			}

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseSwagger();
			//启用中间件服务对swagger-ui，指定Swagger JSON终结点
			app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "TangbulaWorkbench API V1.0"); });

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});

			app.UseResponseCompression();
			app.UseResponseCaching();
			
			app.UseMSFramework();
		}
	}
}