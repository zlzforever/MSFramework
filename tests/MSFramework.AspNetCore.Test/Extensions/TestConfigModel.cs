using MSFramework.Application;

namespace MSFramework.AspNetCore.Test.Extensions
{
	[ConfigModel()]
	public class TestConfigModel
	{
		public string Name { get; set; }
		
		public int Height { get; set; }
	}
}