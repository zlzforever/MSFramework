using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.AutoMapper;
using MSFramework.Ef;
using MSFramework.Ef.Function;
using MSFramework.Ef.MySql;
using MSFramework.EventBus;
using MSFramework.Extensions;
using MSFramework.MySql;
using Template.API.ViewObject;
using Template.Application;
using Template.Domain.AggregateRoot;

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

			services.AddControllersWithViews(x =>
				{
					x.Filters.Add<UnitOfWork>();
					x.Filters.Add<FunctionFilter>();
					x.Filters.Add<HttpGlobalExceptionFilter>();
				})
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
				.AddNewtonsoftJson()
				.AddRazorRuntimeCompilation()
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

			var options = new AppOptions(Configuration);
			services.AddAuthentication(x =>
				{
					x.DefaultScheme = "Cookies";
					x.DefaultChallengeScheme = "oidc";
				})
				.AddCookie("Cookies")
				.AddOpenIdConnect("oidc", x =>
				{
					x.SignInScheme = "Cookies";
					x.Authority = options.Authority;
					x.RequireHttpsMetadata = options.RequireHttpsMetadata;
					x.ClientId = options.ClientId;
					x.SaveTokens = true;
					x.Scope.Add("role");
					// x.GetClaimsFromUserInfoEndpoint = true;
					x.CallbackPath = new PathString("/signin-oidc");
				});

			services.AddGrpc();

			services.AddScoped<AppOptions>();
			services.AddMSFramework(builder =>
			{
				builder.AddEventHandler(typeof(AppOptions));
				builder.AddPassThroughEventBus();
				builder.AddAspNetCoreSession();
				builder.AddAspNetCoreFunction<EfFunctionStore>();
				builder.AddEfAuditStore();
				builder.AddAutoMapper(typeof(AppOptions), typeof(AutoMapperProfile));
				builder.AddMySqlDatabaseMigration();
				builder.AddEntityFramework(ef =>
				{
					// 添加 MyServer 支持
					ef.AddMySqlDbContextOptionsBuilderCreator();
				}, Configuration);
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseMSFramework(x =>
			{
				var options = new AppOptions(Configuration);
				x.UseMySqlDatabaseMigration(typeof(Class1), options.DefaultConnectionString);
			});

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
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseSwagger();
			//启用中间件服务对swagger-ui，指定Swagger JSON终结点
			app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "TangbulaWorkbench API V1.0"); });

			app.UseEndpoints(endpoints =>
			{
				// endpoints.MapGrpcService<GreeterService>();
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});

			app.UseResponseCompression();
			app.UseResponseCaching();
		}
	}
}