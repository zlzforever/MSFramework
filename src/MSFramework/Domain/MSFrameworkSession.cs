using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IMSFrameworkSession
	{
		string UserId { get; }

		string UserName { get; }

		Task CommitAsync();

		void Commit();

		Task<string> GetTokenAsync(string tokenName = "access_token");
	}
}