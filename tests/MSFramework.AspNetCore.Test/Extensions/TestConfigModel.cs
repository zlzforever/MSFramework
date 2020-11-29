using MicroserviceFramework.Configuration;

namespace MSFramework.AspNetCore.Test.Extensions
{
	[OptionsType()]
	public class TestConfigModel
	{
		public string Name { get; set; }
		
		public int Height { get; set; }
	}
}