// using System.Threading;
// using System.Threading.Tasks;
// using MicroserviceFramework;
// using MicroserviceFramework.Mediator;
// using Microsoft.Extensions.DependencyInjection;
// using Xunit;
//
// namespace MSFramework.Tests
// {
// 	// public class Command1 : ICommand
// 	// {
// 	// 	public static int Count;
// 	// }
// 	//
// 	// public class CommandHandler1 : ICommandHandler<Command1>
// 	// {
// 	// 	public Task HandleAsync(Command1 command, CancellationToken cancellationToken = default)
// 	// 	{
// 	// 		Command1.Count += 1;
// 	// 		return Task.CompletedTask;
// 	// 	}
// 	// }
// 	//
// 	// public class CommandHandler2 : ICommandHandler<Command1>
// 	// {
// 	// 	public Task HandleAsync(Command1 command, CancellationToken cancellationToken = default)
// 	// 	{
// 	// 		Command1.Count += 1;
// 	// 		return Task.CompletedTask;
// 	// 	}
// 	// }
// 	//
// 	// public class Command2 : ICommand<int>
// 	// {
// 	// 	public static int Count;
// 	// }
// 	//
// 	// public class Command2Handler : ICommandHandler<Command2, int>
// 	// {
// 	// 	public Task<int> HandleAsync(Command2 command, CancellationToken cancellationToken = default)
// 	// 	{
// 	// 		Command2.Count += 1;
// 	// 		return Task.FromResult(Command2.Count);
// 	// 	}
// 	// }
//
// 	public class Query2 : IRequest<int>
// 	{
// 		public static int Count;
// 	}
//
// 	public class Query2Handler : IRequestHandler<Query2, int>
// 	{
// 		public Task<int> HandleAsync(Query2 query, CancellationToken cancellationToken = default)
// 		{
// 			Query2.Count += 1;
// 			return Task.FromResult(Query2.Count);
// 		}
// 	}
//
// 	public class Query1 : IRequest
// 	{
// 		public static int Count;
// 	}
//
// 	public class Query1Handler1 : IRequestHandler<Query1>
// 	{
// 		public int Order => 1;
//
// 		public Task HandleAsync(Query1 query, CancellationToken cancellationToken = default)
// 		{
// 			Query1.Count += 1;
// 			return Task.CompletedTask;
// 		}
// 	}
//
// 	public class Query1Handler2 : IRequestHandler<Query1>
// 	{
// 		public int Order => 2;
//
// 		public Task HandleAsync(Query1 query, CancellationToken cancellationToken = default)
// 		{
// 			Query1.Count += 1;
// 			return Task.CompletedTask;
// 		}
// 	}
//
//  
//
// 	public class CqrsTests
// 	{
//
// 		[Fact]
// 		public async Task CommandWithoutResponseTest()
// 		{
// 			var serviceCollection = new ServiceCollection();
// 			serviceCollection.AddMicroserviceFramework(x =>
// 			{
// 			});
// 			var serviceProvider = serviceCollection.BuildServiceProvider();
// 			var cqrsProcessor = serviceProvider.GetRequiredService<IMediator>();
// 			for (var i = 0; i < 2; ++i)
// 			{
// 				await cqrsProcessor.ExecuteAsync(new Command1());
// 			}
//
// 			Assert.Equal(2, Command1.Count);
// 		}
//
// 		[Fact]
// 		public async Task CommandWithResponseTest()
// 		{
// 			var serviceCollection = new ServiceCollection();
// 			serviceCollection.AddMicroserviceFramework(x =>
// 			{
// 				//
// 			});
// 			var serviceProvider = serviceCollection.BuildServiceProvider();
// 			var cqrsProcessor = serviceProvider.GetRequiredService<IMediator>();
// 			for (var i = 0; i < 2; ++i)
// 			{
// 				var a = await cqrsProcessor.ExecuteAsync(new Command2());
// 				Assert.Equal(i + 1, a);
// 			}
//
// 			Assert.Equal(2, Command1.Count);
// 		}
//
// 		[Fact]
// 		public async Task QueryWithoutResponseTest()
// 		{
// 			var serviceCollection = new ServiceCollection();
// 			serviceCollection.AddMicroserviceFramework(x =>
// 			{
// 				//
// 			});
// 			var serviceProvider = serviceCollection.BuildServiceProvider();
// 			var cqrsProcessor = serviceProvider.GetRequiredService<IMediator>();
// 			for (var i = 0; i < 2; ++i)
// 			{
// 				await cqrsProcessor.SendAsync(new Query1());
// 			}
//
// 			Assert.Equal(2, Query1.Count);
// 		}
//
// 		[Fact]
// 		public async Task QueryWithResponseTest()
// 		{
// 			var serviceCollection = new ServiceCollection();
// 			serviceCollection.AddMicroserviceFramework(x =>
// 			{
// 				//
// 			});
// 			var serviceProvider = serviceCollection.BuildServiceProvider();
// 			var cqrsProcessor = serviceProvider.GetRequiredService<IMediator>();
// 			for (var i = 0; i < 2; ++i)
// 			{
// 				var a = await cqrsProcessor.SendAsync(new Query2());
// 				Assert.Equal(i + 1, a);
// 			}
//
// 			Assert.Equal(2, Query2.Count);
// 		}
// 	}
// }