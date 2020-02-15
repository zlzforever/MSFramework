using Microsoft.Extensions.Configuration;

namespace MSFramework.AspNetCore.Permission
{
	public class PermissionOptions
	{
		private readonly IConfiguration _configuration;

		public PermissionOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string Service => string.IsNullOrWhiteSpace(_configuration["Audience"])
			? _configuration["Service"]
			: _configuration["Audience"];

		public string SecurityHeader => _configuration["CerberusSecurityHeader"];

		public string Cerberus => _configuration["Cerberus"];

		public int CahceTTL => string.IsNullOrWhiteSpace(_configuration["CahceTTL"])
			? 1
			: int.Parse(_configuration["CahceTTL"]);
	}
}