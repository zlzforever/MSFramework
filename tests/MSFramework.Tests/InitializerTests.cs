using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Initializer;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests
{
	public class Initializer1 : InitializerBase
	{
		public static int Count;

		public override Task InitializeAsync(IServiceProvider serviceProvider)
		{
			Count++;
			return Task.CompletedTask;
		}
	}

	public class Initializer2 : InitializerBase
	{
		public static readonly List<string> Orders = new List<string>();
		public override int Order => 100;

		public override Task InitializeAsync(IServiceProvider serviceProvider)
		{
			Orders.Add("Initializer2");
			return Task.CompletedTask;
		}
	}

	public class Initializer3 : InitializerBase
	{
		public override int Order => 200;

		public override Task InitializeAsync(IServiceProvider serviceProvider)
		{
			Initializer2.Orders.Add("Initializer3");
			return Task.CompletedTask;
		}
	}

	public class Initializer4 : InitializerBase
	{
		public static readonly List<string> Orders = new List<string>();
		public override int Order => 200;

		public override Task InitializeAsync(IServiceProvider serviceProvider)
		{
			Orders.Add("Initializer4");
			return Task.CompletedTask;
		}
	}

	public class Initializer5 : InitializerBase
	{
		public override int Order => 100;

		public override Task InitializeAsync(IServiceProvider serviceProvider)
		{
			Initializer4.Orders.Add("Initializer5");
			return Task.CompletedTask;
		}
	}

	public class InitializerTests
	{
		[Fact]
		public void InitializerTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.AddMicroserviceFramework(
				x => { });
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseMicroserviceFramework();

			Assert.Equal(2, Initializer1.Count);
		}

		[Fact]
		public void InitializerOrderTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.AddMicroserviceFramework(
				x => { });
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseMicroserviceFramework();

			Assert.Equal("Initializer2", Initializer2.Orders[0]);
			Assert.Equal("Initializer3", Initializer2.Orders[1]);
			
			Assert.Equal("Initializer5", Initializer4.Orders[0]);
			Assert.Equal("Initializer4", Initializer4.Orders[1]);
		}
	}
}