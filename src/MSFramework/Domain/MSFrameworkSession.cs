using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IMSFrameworkSession
	{
		string UserId { get; }

		Task CommitAsync();

		void Commit();
	}
}