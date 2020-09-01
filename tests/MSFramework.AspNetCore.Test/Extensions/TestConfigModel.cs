using MicroserviceFramework.Application;

namespace MSFramework.AspNetCore.Test.Extensions
{
	[ConfigType()]
	public class TestConfigModel
	{
		public string Name { get; set; }
		
		public int Height { get; set; }
	}
}