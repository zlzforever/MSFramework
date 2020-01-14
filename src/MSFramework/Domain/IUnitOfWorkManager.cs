using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IUnitOfWorkManager
	{
		void Commit();
		
		Task CommitAsync();
	}
}