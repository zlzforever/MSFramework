using MicroserviceFramework;
using MicroserviceFramework.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MSFramework.Tests
{
	[OptionsType]
	public class MyOptions
	{
		public string Name { get; set; }
		public int Age { get; set; }
	}

	[OptionsType("Person")]
	public class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }
	}

	public class OptionsTest
	{
		[Fact]
		public void Options()
		{
			var serviceCollection = new ServiceCollection();

			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.AddJsonFile("appsettings.json", false, true);
			var configuration = configurationBuilder.Build();
			serviceCollection.AddSingleton<IConfiguration>(configuration);
			serviceCollection.AddOptions();
			serviceCollection.AddMicroserviceFramework(x =>
			{
				x.UseOptions(configuration);
			});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			
			var options = serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value;
			Assert.Equal("lewis", options.Name);
			Assert.Equal(10, options.Age);
			
			var person = serviceProvider.GetRequiredService<IOptions<Person>>().Value;
			Assert.Equal("hi", person.Name);
			Assert.Equal(12, person.Age);
		}
	}
}