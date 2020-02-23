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

		public string CerberusServiceId => _configuration["CerberusServiceId"];

		public string CerberusSecurityHeader => _configuration["CerberusSecurityHeader"];

		public string Cerberus => _configuration["Cerberus"];

		public int CacheTTL => string.IsNullOrWhiteSpace(_configuration["CacheTTL"])
			? 1
			: int.Parse(_configuration["CacheTTL"]);
	}
}