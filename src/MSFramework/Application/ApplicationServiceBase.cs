using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.Application
{
	public class ApplicationServiceBase : IApplicationService
	{
		protected IMSFrameworkSession Session { get; }

		protected ILogger Logger { get; }

		protected ApplicationServiceBase(IMSFrameworkSession session, ILogger logger)
		{
			Session = session;
			Logger = logger;
		}

		protected async Task SetBearer(HttpClient client, string tokenName = "access_token")
		{
			var accessToken = await Session.GetTokenAsync(tokenName);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
		}
	}
}