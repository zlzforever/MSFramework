using Microsoft.Extensions.Configuration;

namespace MicroserviceFramework.AspNetCore.AccessControl
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
			? 10
			: int.Parse(_configuration["AccessControl:CacheTTL"]);

		public string Authority => _configuration["AccessControl:Authority"];
		public string ClientId => _configuration["AccessControl:ClientId"];
		public string ClientSecret => _configuration["AccessControl:ClientSecret"];
		public string HttpClient { get; set; } = "MicroserviceFramework.AspNetCore.AccessControl.HttpClient";
	}
}