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

		protected ApiResult ApiResult(dynamic value = null, string msg = "")
		{
			return new ApiResult(value, msg);
		}

		protected ErrorApiResult ErrorApiResult(string msg = "", int code = 20000)
		{
			HttpContext.Response.StatusCode = 400;
			return new ErrorApiResult(msg, code);
		}
	}
}