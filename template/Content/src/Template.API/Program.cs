namespace Template.API;

/// <summary>API 组合根，使用单一 WebApplicationBuilder 注册框架与业务依赖。</summary>
public partial class Program
{
    /// <summary>启动 HTTP 宿主，配置由默认 JSON、User Secrets 和环境变量提供。</summary>
    /// <param name="args">标准 ASP.NET Core 命令行配置参数。</param>
    /// <returns>宿主停止时完成的任务。</returns>
    /// <exception cref="InvalidOperationException">数据库或认证配置缺失时抛出。</exception>
    public static async Task Main(string[] args)
    {
        var builder = CreateWebApplicationBuilder(args);
        await using var app = builder.Build();
        app.Configure();
        await app.RunAsync();
    }

    /// <summary>构建运行时与 EF 设计时共享的服务注册，不连接数据库或自动探测服务器版本。</summary>
    /// <param name="args">命令行参数，不允许为空。</param>
    /// <returns>已注册所有应用服务的构建器。</returns>
    /// <exception cref="ArgumentNullException">参数数组为空。</exception>
    /// <exception cref="InvalidOperationException">所需配置缺失。</exception>
    public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.ConfigureApplication(builder.Configuration);
        return builder;
    }
}
