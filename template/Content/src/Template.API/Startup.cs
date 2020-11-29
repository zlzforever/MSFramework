using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Infrastructure;
using MicroserviceFramework.Audit;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Configuration;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
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

			services.AddOptions(Configuration);

			// SwaggerUI require some service from views,
			// if your project a pure api, please use AddControllers
			services.AddControllers(x =>
				{
					x.Filters.AddUnitOfWork();
					x.Filters.AddFunctionFilter();
					x.Filters.AddAudit();
					x.Filters.AddGlobalException();
					x.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
				})
				.ConfigureInvalidModelStateResponse()
				.AddNewtonsoftJson(x =>
				{
					x.SerializerSettings.Converters.Add(new ObjectIdConverter());
					x.SerializerSettings.Converters.Add(new EnumerationConverter());
					x.SerializerSettings.ContractResolver = new CompositeContractResolver
					{
						new EnumerationContractResolver(),
						new CamelCasePropertyNamesContractResolver()
					};
				});

			services.AddHealthChecks();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0",
					new OpenApiInfo {Version = "v1.0", Description = "Template API V1.0"});
				c.CustomSchemaIds(x => x.FullName);
				c.AddEnumerationDoc(typeof(TemplateOptions).Assembly).AddObjectIdDoc();
			});

			services.AddResponseCompression();
			services.AddResponseCaching();

			services.AddRouting(x => { x.LowercaseUrls = true; });

#if !DEBUG
			var options = new TemplateOptions(Configuration);
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

			services.AddMicroserviceFramework(builder =>
			{
				// builder.UseNewtonsoftJson();
				builder.UseAutoMapper();
				builder.UseCqrs();
				builder.UseBaseX();
				//builder.UseAccessControl(Configuration);
				// 启用审计服务
				builder.UseAudit();
				builder.UseAspNetCore();
				builder.UseEntityFramework(x =>
				{
					// 添加 MySql 支持
					x.AddMySql<TemplateDbContext>(Configuration);
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
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