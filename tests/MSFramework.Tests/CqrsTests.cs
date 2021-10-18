using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Application.CQRS;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests
{
	public class Command1 : ICommand
	{
		public static int Count;
	}

	public class CommandHandler1 : ICommandHandler<Command1>
	{
		public Task HandleAsync(Command1 command, CancellationToken cancellationToken = default)
		{
			Command1.Count += 1;
			return Task.CompletedTask;
		}
	}

	public class CommandHandler2 : ICommandHandler<Command1>
	{
		public Task HandleAsync(Command1 command, CancellationToken cancellationToken = default)
		{
			Command1.Count += 1;
			return Task.CompletedTask;
		}
	}

	public class Command2 : ICommand<int>
	{
		public static int Count;
	}

	public class Command2Handler : ICommandHandler<Command2, int>
	{
		public Task<int> HandleAsync(Command2 command, CancellationToken cancellationToken = default)
		{
			Command2.Count += 1;
			return Task.FromResult(Command2.Count);
		}
	}

	public class Query2 : IQuery<int>
	{
		public static int Count;
	}

	public class Query2Handler : IQueryHandler<Query2, int>
	{
		public Task<int> HandleAsync(Query2 query, CancellationToken cancellationToken = default)
		{
			Query2.Count += 1;
			return Task.FromResult(Query2.Count);
		}
	}

	public class Query1 : IQuery
	{
		public static int Count;
	}

	public class Query1Handler1 : IQueryHandler<Query1>
	{
		public int Order => 1;

		public Task HandleAsync(Query1 query, CancellationToken cancellationToken = default)
		{
			Query1.Count += 1;
			return Task.CompletedTask;
		}
	}

	public class Query1Handler2 : IQueryHandler<Query1>
	{
		public int Order => 2;

		public Task HandleAsync(Query1 query, CancellationToken cancellationToken = default)
		{
			Query1.Count += 1;
			return Task.CompletedTask;
		}
	}

	public class CommandHandler1<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
	{
		public virtual Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}
	}

	public class CqrsTests
	{

		[Fact]
		public async Task CommandWithoutResponseTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMicroserviceFramework(x =>
			{
			});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var cqrsProcessor = serviceProvider.GetRequiredService<ICqrsProcessor>();
			for (var i = 0; i < 2; ++i)
			{
				await cqrsProcessor.ExecuteAsync(new Command1());
			}

			Assert.Equal(2, Command1.Count);
		}

		[Fact]
		public async Task CommandWithResponseTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMicroserviceFramework(x =>
			{
				//
			});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var cqrsProcessor = serviceProvider.GetRequiredService<ICqrsProcessor>();
			for (var i = 0; i < 2; ++i)
			{
				var a = await cqrsProcessor.ExecuteAsync(new Command2());
				Assert.Equal(i + 1, a);
			}

			Assert.Equal(2, Command1.Count);
		}

		[Fact]
		public async Task QueryWithoutResponseTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMicroserviceFramework(x =>
			{
				//
			});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var cqrsProcessor = serviceProvider.GetRequiredService<ICqrsProcessor>();
			for (var i = 0; i < 2; ++i)
			{
				await cqrsProcessor.QueryAsync(new Query1());
			}

			Assert.Equal(2, Query1.Count);
		}

		[Fact]
		public async Task QueryWithResponseTest()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddMicroserviceFramework(x =>
			{
				//
			});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var cqrsProcessor = serviceProvider.GetRequiredService<ICqrsProcessor>();
			for (var i = 0; i < 2; ++i)
			{
				var a = await cqrsProcessor.QueryAsync(new Query2());
				Assert.Equal(i + 1, a);
			}

			Assert.Equal(2, Query2.Count);
		}
	}
}