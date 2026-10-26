using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Template.API;

/// <summary>注册 JWT 业务认证及官方 Dapr App API Token 认证，两类端点使用独立策略。</summary>
public static class IdentityServerExtensions
{
    /// <summary>启用 HTTPS 签发者、受众及 Scope 校验，不通过编译模式关闭认证。</summary>
    /// <param name="services">服务集合，不允许为空。</param>
    /// <param name="configuration">配置，必须包含 HTTPS Authority、Audience 和 Scope。</param>
    /// <exception cref="ArgumentNullException">参数为空。</exception>
    /// <exception cref="InvalidOperationException">认证配置缺失或签发者不是 HTTPS 地址。</exception>
    public static void ConfigureIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var authority = configuration["Authentication:Authority"];
        var audience = configuration["Authentication:Audience"];
        var scope = configuration["Authentication:Scope"];
        if (!Uri.TryCreate(authority, UriKind.Absolute, out var authorityUri) ||
            authorityUri.Scheme != Uri.UriSchemeHttps ||
            string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(scope))
        {
            throw new InvalidOperationException("请配置 HTTPS Authentication:Authority、Audience 和 Scope。");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters.ValidateAudience = true;
            })
            .AddDapr();
        services.AddAuthorization(options =>
        {
            options.AddDapr();
            options.AddPolicy("Api", policy => policy
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.FindAll("scope").Any(claim =>
                    claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Contains(scope, StringComparer.Ordinal))));
        });
    }
}
