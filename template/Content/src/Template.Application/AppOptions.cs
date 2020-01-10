using Microsoft.Extensions.Configuration;

namespace Template.Application
{
	public class AppOptions
	{
		private readonly IConfiguration _configuration;

		public AppOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string ClientId => _configuration["ClientId"];

		public string Authority => _configuration["Authority"];

		public bool RequireHttpsMetadata => bool.Parse(_configuration["RequireHttpsMetadata"]);
		
		public int PageLimit => string.IsNullOrWhiteSpace(_configuration["Page:MaxLimit"])
			? 15
			: int.Parse(_configuration["Page:MaxLimit"]);

		public string DefaultConnectionString => _configuration["DbContexts:AppDbContext:ConnectionString"];
	}
}