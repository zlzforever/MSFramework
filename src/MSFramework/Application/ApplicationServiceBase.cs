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
	}
}