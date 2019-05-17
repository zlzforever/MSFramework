using System;

namespace MSFramework.Command
{
	public interface ICommand<TResponse>
	{
	}

	public interface ICommand : ICommand<bool>
	{
	}
}