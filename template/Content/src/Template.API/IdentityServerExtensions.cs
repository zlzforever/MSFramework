using System.Threading.Tasks;
using MicroserviceFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Template.API;

public static class IdentityServerExtensions
{
    public static void ConfigureIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                };
                options.Authority = configuration["Authority"];
                options.RequireHttpsMetadata = configuration["RequireHttpsMetadata"] == "true";
                options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
            });

        // adds an authorization policy to make sure the token is for scope 'api1'
        var apiName = configuration["ApiName"];
        if (string.IsNullOrEmpty(apiName))
        {
            throw new MicroserviceFrameworkException("ApiName is null or empty");
        }
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Jwt", policy =>
            {
                policy.AddAuthenticationSchemes("Bearer");
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", apiName);
            });
        });
    }
}
