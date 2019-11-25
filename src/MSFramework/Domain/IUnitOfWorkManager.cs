using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IUnitOfWorkManager
	{
		Task CommitAsync();
	}
}