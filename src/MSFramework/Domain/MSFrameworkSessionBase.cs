using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public abstract class MSFrameworkSessionBase : IMSFrameworkSession
	{
		public abstract string UserId { get; }

		public abstract string UserName { get; }

		public abstract Task<string> GetTokenAsync(string tokenName = "access_token");
	}
}