using MicroserviceFramework.Configuration;

namespace MSFramework.AspNetCore.Test.Extensions
{
	[Options()]
	public class TestConfigModel
	{
		public string Name { get; set; }
		
		public int Height { get; set; }
	}
}