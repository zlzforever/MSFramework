using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using MicroserviceFramework.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Template.API;
using Template.Application.Products;
using Template.Application.Products.Queries;
using Template.Domain.Aggregates;
using Xunit;

namespace Template.Tests;

/// <summary>真实 HTTP 管道回归，不访问数据库、认证服务器或 Dapr；覆盖安全默认值、CORS 与绑定。</summary>
/// <param name="factory">为当前测试类共享的隔离宿主。</param>
public class ApiTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    /// <summary>健康检查无需身份，业务读取默认拒绝匿名。</summary>
    [Fact]
    public async Task HealthIsPublicAndProductsRequireAuthentication()
    {
        using var client = factory.CreateClient();
        using var health = await client.GetAsync("/healthcheck");
        using var products = await client.GetAsync("/api/v1.0/products");
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, products.StatusCode);
    }

    /// <summary>真实签名令牌仍必须满足本 API 的受众与 Scope。</summary>
    /// <param name="audience">测试受众。</param>
    /// <param name="scope">测试权限字符串，允许多个空格分隔值。</param>
    /// <param name="expectedStatus">预期 HTTP 状态码。</param>
    [Theory]
    [InlineData("other-api", "template-api", 401)]
    [InlineData("template-api", "other-scope", 403)]
    [InlineData("template-api", "other template-api", 200)]
    public async Task AudienceAndScopeAreEnforced(string audience, string scope, int expectedStatus)
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateToken(audience, scope));
        using var response = await client.GetAsync("/api/v1.0/products?page=2&limit=7");
        Assert.Equal(expectedStatus, (int)response.StatusCode);
        if (response.IsSuccessStatusCode)
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(2, json.RootElement.GetProperty("data").GetProperty("page").GetInt32());
            Assert.Equal(7, json.RootElement.GetProperty("data").GetProperty("limit").GetInt32());
        }
    }

    /// <summary>非法枚举输入在进入业务及工作单元之前返回统一 400。</summary>
    [Fact]
    public async Task InvalidEnumerationReturnsBadRequest()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateToken());
        using var response = await client.PostAsJsonAsync("/api/v1.0/products", new
        {
            name = "Product", price = 100, productType = "Unknown"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>白名单预检正常执行，未知来源不获得跨域许可。</summary>
    /// <param name="origin">浏览器来源。</param>
    /// <param name="allowed">是否应返回跨域许可头。</param>
    [Theory]
    [InlineData("https://frontend.example", true)]
    [InlineData("https://untrusted.example", false)]
    public async Task CorsUsesExplicitOrigins(string origin, bool allowed)
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1.0/products");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        using var response = await client.SendAsync(request);
        Assert.Equal(allowed, response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
    }

    /// <summary>Dapr 回调不能被匿名 HTTP 请求调用。</summary>
    [Fact]
    public async Task DaprCallbackRejectsAnonymousCalls()
    {
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/events/products/created", new { productId = ObjectId.GenerateNewId().ToString() });
        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateToken());
        using var jwtResponse = await client.PostAsJsonAsync("/events/products/created", new { productId = ObjectId.GenerateNewId().ToString() });
        Assert.True(jwtResponse.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    /// <summary>商品领域模型在脱离 MVC 时仍校验不变量。</summary>
    [Fact]
    public void ProductHasOfflineIdAndCreationEvent()
    {
        var product = Product.Create(" Product ", 100, ProductType.Home);
        Assert.NotEqual(ObjectId.Empty, product.Id);
        Assert.Equal("Product", product.Name);
        Assert.Single(product.GetDomainEvents());
        Assert.Throws<MicroserviceFramework.MicroserviceFrameworkFriendlyException>(() =>
            Product.Create(" ", 100, ProductType.Home));
    }
}

/// <summary>使用真实 JWT 校验与固定只读查询替身的宿主，随机签名密钥只存于测试内存。</summary>
public class ApiTestFactory : WebApplicationFactory<Program>
{
    private readonly SymmetricSecurityKey _key = new(RandomNumberGenerator.GetBytes(32));

    /// <summary>注入非敏感测试配置，不覆盖产品中的认证或 CORS 中间件。</summary>
    /// <param name="builder">测试宿主构建器。</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("DbContexts:0:ConnectionString", "Server=localhost;Database=template_test;User ID=test");
        builder.UseSetting("Authentication:Authority", "https://identity.example");
        builder.UseSetting("Cors:AllowedOrigins:0", "https://frontend.example");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IProductQueries>();
            services.AddScoped<IProductQueries, StubProductQueries>();
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // 使用本地签名验证，避免 OIDC 元数据网络请求；保留受众、有效期及策略校验。
                options.Configuration = new OpenIdConnectConfiguration { Issuer = "https://identity.example" };
                options.TokenValidationParameters.IssuerSigningKey = _key;
                options.TokenValidationParameters.ValidIssuer = "https://identity.example";
            });
        });
    }

    /// <summary>签发短期测试令牌，供认证及授权边界回归使用。</summary>
    /// <param name="audience">令牌受众，默认为本 API。</param>
    /// <param name="scope">权限字符串，默认为本 API Scope。</param>
    /// <returns>仅在当前测试宿主有效的 JWT。</returns>
    public string CreateToken(string audience = "template-api", string scope = "template-api") =>
        new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("https://identity.example", audience,
            [new Claim("sub", "test-user"), new Claim("scope", scope)], expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)));

    private sealed class StubProductQueries : IProductQueries
    {
        /// <inheritdoc/>
        public Task<ProductOut?> GetAsync(ObjectId id, CancellationToken cancellationToken) => Task.FromResult<ProductOut?>(null);

        /// <inheritdoc/>
        public Task<PaginationResult<ProductOut>> PagedAsync(PagedProductQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(new PaginationResult<ProductOut>(query.Page, query.Limit, 0, []));
    }
}
