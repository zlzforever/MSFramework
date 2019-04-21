using System.Threading.Tasks;
using MSFramework.EventBus;

namespace MSFramework.CQRS
{
	public interface ICommandBus
	{
		Task PublishAsync(ICommand @event);

		void Subscribe<T, TH>()
			where T : class, ICommand
			where TH : ICommandHandler<T>;

		void Unsubscribe<T, TH>()
			where T : class, ICommand
			where TH : ICommandHandler<T>;
	}
}