using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using MSFramework.Http;

namespace MSFramework.AspNetCore
{
	public class MSFrameworkApiControllerBase : ControllerBase, IAsyncResultFilter
	{
		protected IMSFrameworkSession Session { get; }

		protected ILogger Logger { get; }

		protected MSFrameworkApiControllerBase(IMSFrameworkSession session, ILogger logger)
		{
			Session = session;
			Logger = logger;
		}

		protected ApiResult ApiResult(dynamic value = null, string msg = "")
		{
			return new ApiResult(value, msg);
		}

		protected ErrorApiResult ErrorApiResult(string msg = "", int code = 20000)
		{
			HttpContext.Response.StatusCode = 400;
			return new ErrorApiResult(msg, code);
		}

		[NonAction]
		public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			if (context.Result is EmptyResult)
			{
				context.Result = new JsonResult(ApiResult());
			}

			return next();
		}
	}
}