using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public class MSFrameworkControllerBase : Controller
	{
		protected IMSFrameworkSession Session { get; }

		protected ILogger Logger { get; }

		protected MSFrameworkControllerBase(IMSFrameworkSession session, ILogger logger)
		{
			Session = session;
			Logger = logger;
		}
	}
}