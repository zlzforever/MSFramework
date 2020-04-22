using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MSFramework;
using MSFramework.AspNetCore;
#if !DEBUG
using MSFramework.AspNetCore.Permission;
#endif
using MSFramework.AutoMapper;
using MSFramework.Ef;
using MSFramework.Ef.Function;
using MSFramework.Ef.MySql;
using MSFramework.Extensions;
using MSFramework.MySql;
using Template.API.ViewObject;
using Template.Application.Extensions;
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
			Configuration.Print();

			// SwaggerUI require some service from views,
			// if your project a pure api, please use AddControllers
			services.AddControllers(x =>
				{
					x.Filters.Add<UnitOfWork>();
					x.Filters.Add<FunctionFilter>();
					x.Filters.Add<HttpGlobalExceptionFilter>();
				})
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
				.AddNewtonsoftJson()
				.ConfigureApiBehaviorOptions(x =>
				{
					x.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Instance;
				});

			services.AddHealthChecks();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0",
					new OpenApiInfo {Version = "v1.0", Description = "Template API V1.0"});
			});

			services.AddResponseCompression();
			services.AddResponseCaching();

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
				builder.AddEventBus(typeof(AppOptions), typeof(MSFrameworkSessionExtensions));
				builder.AddAspNetCore();
				builder.AddAspNetCoreFunction<EfFunctionStore>();
				builder.AddEfAuditStore();
#if !DEBUG
				builder.AddPermission();
#endif
				builder.AddAutoMapper(typeof(AppOptions), typeof(AutoMapperProfile));
				builder.AddDatabaseMigration<MySqlDatabaseMigration>(typeof(AppDbContext),
					"Database='template';Data Source=localhost;password=1qazZAQ!;User ID=root;Port=3306;Allow User Variables=true");
				builder.AddEntityFramework(ef =>
				{
					// 添加 MyServer 支持
					ef.AddMySql<AppDbContext>();
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseMSFramework();

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
		}
	}
}