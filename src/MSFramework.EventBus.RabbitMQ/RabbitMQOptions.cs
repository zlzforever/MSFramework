using Microsoft.Extensions.Configuration;

namespace MSFramework.EventBus.RabbitMQ
{
	public class RabbitMQOptions
	{
		private readonly IConfiguration _configuration;

		public RabbitMQOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string BrokerName => string.IsNullOrWhiteSpace(_configuration["RabbitMQ:BrokerName"])
			? "EventBus"
			: _configuration["RabbitMQ:BrokerName"];

		public string HostName => _configuration["RabbitMQ:HostName"];

		public int Port => int.Parse(_configuration["RabbitMQ:Port"]);

		public string UserName => _configuration["RabbitMQ:UserName"];

		public string Password => _configuration["RabbitMQ:Password"];

		public int RetryCount => string.IsNullOrWhiteSpace(_configuration["RabbitMQ:RetryCount"])
			? 5
			: int.Parse(_configuration["RabbitMQ:RetryCount"]);
	}
}