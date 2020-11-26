using MicroserviceFramework.Configuration;
using Microsoft.Extensions.Configuration;

namespace Ordering.Domain
{
	[Options]
	public class AppOptions
	{
		private readonly IConfiguration _configuration;

		public AppOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string Audience { get; set; }

		public string DefaultConnectionString => _configuration["DbContexts:OrderingContext:ConnectionString"];
	}

	[Options("Email")]
	public class EmailOptions
	{
		public string Address { get; set; }
	}
}