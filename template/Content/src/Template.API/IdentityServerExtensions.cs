using System.Threading.Tasks;
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
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateAudience = false
				};
			});

		// adds an authorization policy to make sure the token is for scope 'api1'
		services.AddAuthorization(options =>
		{
			options.AddPolicy("Jwt", policy =>
			{
				policy.AddAuthenticationSchemes("Bearer");
				policy.RequireAuthenticatedUser();
				policy.RequireClaim("scope", configuration["ApiName"]);
			});
		});
	}
}