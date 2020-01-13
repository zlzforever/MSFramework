using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Data;
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