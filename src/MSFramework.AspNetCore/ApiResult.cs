using Microsoft.AspNetCore.Mvc;

namespace MSFramework.AspNetCore
{
	public class ApiResult : JsonResult
	{
		public ApiResult() : base(new
		{
			success = true,
			code = 100,
			msg = string.Empty,
			data = string.Empty
		})
		{
		}

		public ApiResult(object value, string msg = "") : base(new
		{
			success = true,
			code = 100,
			msg,
			data = value
		})
		{
		}
	}

	public class FailedApiResult : ApiResult
	{
		public FailedApiResult(string msg) : base(new
		{
			success = false,
			code = 400,
			msg,
			data = string.Empty
		})
		{
		}
	}
}