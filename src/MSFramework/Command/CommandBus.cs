using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MSFramework.Command
{
	public class CommandBus : ICommandBus
	{
		private readonly ILogger _logger;
		private readonly IServiceProvider _serviceProvider;

		public CommandBus(ILogger<CommandBus> logger, IServiceProvider serviceProvider)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// 发送命令
		/// </summary>
		/// <typeparam name="TCommand"></typeparam>
		/// <param name="command"></param>
		public async Task<bool> SendAsync<TCommand>(TCommand command) where TCommand : class, ICommand
		{
			if (command == null)
				return false;

			if (_serviceProvider.GetService(typeof(ICommandHandler<TCommand>)) is ICommandHandler<TCommand> handler)
			{
				try
				{
					var interceptors = _serviceProvider.GetServices(typeof(ICommandInterceptor<TCommand>))
						.Select(x => (ICommandInterceptor<TCommand>) x).ToList();
					if (interceptors.Count > 0)
					{
						var start = new CommandInterceptorHandler<TCommand>
						{
							Current = interceptors[0]
						};
						var current = start;
						for (int j = 1; j < interceptors.Count; ++j)
						{
							current.Next = new CommandInterceptorHandler<TCommand>
							{
								Current = interceptors[j]
							};
							current = current.Next;
						}
						current.Next = new FinalCommandInterceptorHandler<TCommand>(handler);
						await start.ExecuteAsync(command);
					}
					else
					{
						await handler.ExecuteAsync(command);
					}

					return true;
				}
				catch (Exception e)
				{
					_logger.LogError($"Execute command failed: {e}");
					return false;
				}
			}
			else
			{
				_logger.LogError($"找不到命令{nameof(command)}的处理类，或者 IOC 未注册。");
				return false;
			}
		}
	}
}