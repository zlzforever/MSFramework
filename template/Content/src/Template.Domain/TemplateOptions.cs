using Microsoft.Extensions.Configuration;

namespace Template.Domain
{
	public class TemplateOptions
	{
		private readonly IConfiguration _configuration;

		public TemplateOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string ApiName => _configuration["ApiName"];
		public string ApiSecret => _configuration["ApiSecret"];
		public string Authority => _configuration["Authority"];
		public bool RequireHttpsMetadata => bool.Parse(_configuration["RequireHttpsMetadata"]);

		public string DefaultConnectionString => _configuration["DbContexts:AppDbContext:ConnectionString"];
	}
}