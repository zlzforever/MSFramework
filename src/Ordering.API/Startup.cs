using System;
using System.IO;
using System.Linq;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Infrastructure;
using MicroserviceFramework.Audit;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Configuration;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.MySql;
using MicroserviceFramework.Newtonsoft;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Ordering.Application.Queries;
using Ordering.Domain.AggregateRoots;
using Ordering.Infrastructure;
using Serilog;

namespace Ordering.API
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			var type = typeof(OrderingQuery);
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddOptions(Configuration);

			Configuration.Print(x => Log.Logger.Information(x));

			services.AddScoped<DbContext, OrderingContext>();

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
					// x.SerializerSettings.ContractResolver = new CompositeContractResolver
					// {
					// 	new EnumerationContractResolver(),
					// 	new CamelCasePropertyNamesContractResolver()
					// };
				});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1.0", new OpenApiInfo {Version = "v1.0", Description = "Ordering API V1.0"});
				c.CustomSchemaIds(type => type.FullName);
				c.AddEnumerationDoc(typeof(Address).Assembly).AddObjectIdDoc();
				// c.MapType<ObjectId>(() => new OpenApiSchema
				// {
				// 	//Type = "string", Default = new OpenApiString(ObjectId.Empty.ToString()),
				// });
			});
			services.AddHealthChecks();

			services.AddMicroserviceFramework(builder =>
			{
				builder.UseNewtonsoftJson();
				builder.UseAutoMapper();
				builder.UseCqrs();
				builder.UseBaseX();
				//builder.UseAccessControl(Configuration);
				// builder.UseRabbitMQEventDispatcher(new RabbitMQOptions(), typeof(UserCheckoutAcceptedEvent));
				// 启用审计服务
				builder.UseAudit();
				// builder.UseMySqlMigrator(typeof(OrderingContext),
				// 	"Database='ordering';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306;");

				builder.UseAspNetCore();
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

			app.UseMicroserviceFramework();
		}
	}
}