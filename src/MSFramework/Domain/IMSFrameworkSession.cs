using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MSFramework.Domain
{
	public interface IMSFrameworkSession
	{
		string UserId { get; }

		string UserName { get; }

		HttpContext HttpContext { get; }

		Task<string> GetTokenAsync(string tokenName = "access_token");
	}
}