using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
		
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			Session.Commit();
		}
	}
}