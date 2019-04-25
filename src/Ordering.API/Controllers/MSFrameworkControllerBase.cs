using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MSFramework.Domain;

namespace Ordering.API.Controllers
{
	public class MSFrameworkControllerBase : Controller
	{
		protected IMSFrameworkSession Session { get; }

		protected MSFrameworkControllerBase(IMSFrameworkSession session)
		{
			Session = session;
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			base.OnActionExecuted(context);

			Session.CommitAsync().ConfigureAwait(false);
		}
	}
}