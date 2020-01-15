using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using MSFramework.Http;

namespace MSFramework.AspNetCore
{
	public class MSFrameworkApiControllerBase : ControllerBase
	{
		protected IMSFrameworkSession Session { get; }

		protected ILogger Logger { get; }

		protected MSFrameworkApiControllerBase(IMSFrameworkSession session, ILogger logger)
		{
			Session = session;
			Logger = logger;
		}

		protected IActionResult Ok(dynamic value, string msg = "")
		{
			return new JsonResult(new ApiResult(value, msg));
		}

		protected IActionResult Failed(string msg = "", int code = 20000)
		{
			HttpContext.Response.StatusCode = 400;
			return new JsonResult(new ApiResult(false, null, msg, code));
		}
	}
}