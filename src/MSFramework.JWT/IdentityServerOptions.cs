using Microsoft.Extensions.Configuration;

namespace MSFramework.IdentityServer4
{
	public class IdentityServerOptions
	{
		private readonly IConfiguration _configuration;

		public IdentityServerOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string AccessTokenJwtType => _configuration["IdentityServer:AccessTokenJwtType"];
	}
}