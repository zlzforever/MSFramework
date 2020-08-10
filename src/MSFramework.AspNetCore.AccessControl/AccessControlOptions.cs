using Microsoft.Extensions.Configuration;

namespace MSFramework.AspNetCore.AccessControl
{
	public class AccessControlOptions
	{
		private readonly IConfiguration _configuration;

		public AccessControlOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string ServiceUrl => _configuration["AccessControl:ServiceUrl"];

		public int CacheTTL => string.IsNullOrWhiteSpace(_configuration["AccessControl:CacheTTL"])
			? 5
			: int.Parse(_configuration["AccessControl:CacheTTL"]);
	}
}