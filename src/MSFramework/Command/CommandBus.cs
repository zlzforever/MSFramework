using System;
using System.Threading.Tasks;
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
		public async Task Send<TCommand>(TCommand command) where TCommand : class, ICommand
		{
			if (command == null)
				return;

			var handler = _serviceProvider.GetService(typeof(ICommandHandler<TCommand>));

			if (handler != null)
			{
				await ((ICommandHandler<TCommand>) handler).ExecuteAsync(command);
			}
			else
			{
				throw new Exception($"找不到命令{nameof(command)}的处理类，或者 IOC 未注册。");
			}
		}
	}
}