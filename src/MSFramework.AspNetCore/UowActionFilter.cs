using Microsoft.AspNetCore.Mvc.Filters;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public class UowActionFilter : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			var session =
				context.HttpContext.RequestServices.GetService(typeof(IMSFrameworkSession)) as IMSFrameworkSession;
			session?.Commit();
		}
	}
}