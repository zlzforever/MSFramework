using Microsoft.Extensions.Configuration;
using MSFramework.Application;

namespace Ordering.Domain
{
	[ConfigType]
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

	[ConfigType("Email")]
	public class EmailOptions
	{
		public string Address { get; set; }
	}
}