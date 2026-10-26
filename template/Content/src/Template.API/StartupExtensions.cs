using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Filters;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Ef.MySql;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Text.Json;
using Microsoft.EntityFrameworkCore;
using Template.Infrastructure;

namespace Template.API;

/// <summary>注册 MVC、MySQL、框架服务并配置中间件顺序，依赖配置提供连接与认证信息。</summary>
public static class StartupExtensions
{
    private const string CORS_POLICY = "Frontend";

    /// <summary>注册应用服务，框架按 Template 程序集前缀发现仓储和处理器。</summary>
    /// <param name="services">宿主服务集合，不允许为空。</param>
    /// <param name="configuration">应用配置，不允许为空；必须包含数据库及认证配置。</param>
    /// <exception cref="ArgumentNullException">服务集合或配置为空。</exception>
    /// <exception cref="InvalidOperationException">连接字符串或认证配置缺失。</exception>
    public static void ConfigureApplication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var settings = configuration.GetDbContextSettings<TemplateDbContext>();
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException("请通过 User Secrets 或 DbContexts__0__ConnectionString 配置数据库连接。");
        }

        services.AddHttpContextAccessor();
        services.AddControllers(options =>
            {
                options.Filters.AddGlobalException().AddAudit().AddUnitOfWork().AddResponseWrapper();
                options.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new EnumerationModelBinderProvider());
            })
            .ConfigureInvalidModelStateResponse()
            .AddJsonOptions(options => options.JsonSerializerOptions.AddDefaultConverters())
            .AddDapr();

        // 固定服务器兼容版本，禁止启动期 AutoDetect 访问数据库；部署时按实际 MySQL 版本调整。
        services.AddDbContextPool<TemplateDbContext>(configuration, (dbSettings, _, options) =>
            options.UseMySql(new MySqlServerVersion(new Version(8, 0, 36)), mysql => mysql.Load(dbSettings)));
        services.AddMicroserviceFramework(builder =>
        {
            builder.UseDependencyInjectionLoader();
            builder.UseAspNetCoreExtension();
            builder.UseScopeServiceProvider();
            builder.UseEntityFramework();
            builder.UseEfAuditing<TemplateDbContext>();
        }, "Template");
        services.ConfigureIdentityServer(configuration);
        services.AddCors(options => options.AddPolicy(CORS_POLICY, policy => policy
            .WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader().AllowAnyMethod()));
        services.AddHealthChecks();
    }

    /// <summary>初始化框架并配置路由、CORS、认证和 Dapr 回调；不在启动时播种业务数据。</summary>
    /// <param name="app">已构建的应用，不允许为空。</param>
    /// <exception cref="ArgumentNullException">应用为空。</exception>
    public static void Configure(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseCors(CORS_POLICY);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCloudEvents();
        app.MapHealthChecks("/healthcheck").AllowAnonymous();
        // 发现端点供 sidecar 本机访问，部署网关必须禁止外部访问 /dapr/subscribe。
        app.MapSubscribeHandler();
        app.MapControllers();
        app.UseMicroserviceFramework();
    }
}
