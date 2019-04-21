using System.Threading.Tasks;
using MSFramework.EventBus;

namespace MSFramework.CQRS
{
	public interface ICommandHandler<in T> where T : class, ICommand
	{
	}
}