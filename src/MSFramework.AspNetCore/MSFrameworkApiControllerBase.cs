using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

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
			return new ApiResult(value, msg);
		}

		protected IActionResult Failed(string msg = "", int code = 20000)
		{
			return new ApiResult(new
			{
				success = false,
				code,
				msg
			})
			{
				StatusCode = 500
			};
		}
	}
}