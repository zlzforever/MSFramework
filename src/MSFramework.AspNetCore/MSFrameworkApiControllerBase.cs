using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

		protected IActionResult PagedResult<TEntity>(PagedQueryResult<TEntity> result)
		{
			return new PagedApiResult<TEntity>(result);
		}

		protected IActionResult PagedResult<TEntity, TDTO>(PagedQueryResult<TEntity> result)
		{
			var mapper = HttpContext.RequestServices.GetRequiredService<IMapper>();
			var output = new PagedQueryResult<TDTO>
			{
				Limit = result.Limit,
				Total = result.Total,
				Page = result.Page,
				Entities = mapper.Map<List<TDTO>>(result.Entities)
			};
			return PagedResult(output);
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