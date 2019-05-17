using System.Threading.Tasks;

namespace MSFramework.Command
{
	public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
	{
		Task<TResponse> HandleAsync(TCommand message);
	}

	class MyCommand : ICommand<int>
	{
	}

	class Handler : ICommandHandler<MyCommand, int>
	{
		public Task<int> HandleAsync(MyCommand message)
		{
			return Task.FromResult<int>(1);
		}
	}
}