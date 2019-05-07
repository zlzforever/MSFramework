using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IMSFrameworkSession
	{
		string UserId { get; }
		
		string UserName { get; }

		Task CommitAsync();

		void Commit();
	}
}