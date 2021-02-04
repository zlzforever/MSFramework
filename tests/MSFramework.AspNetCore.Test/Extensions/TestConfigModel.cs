using MicroserviceFramework.Configuration;

namespace MSFramework.AspNetCore.Test.Extensions
{
	[OptionsType("TestConfigModel")]
	public class TestConfigModel
	{
		public string Name { get; set; }

		public int Height { get; set; }
	}
}