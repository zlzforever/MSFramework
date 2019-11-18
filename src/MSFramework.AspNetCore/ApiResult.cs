using Microsoft.AspNetCore.Mvc;

namespace MSFramework.AspNetCore
{
	public class ApiResult : JsonResult
	{
		protected ApiResult(object value) : base(value)
		{
		}
		
		public ApiResult() : base(new
		{
			success = true,
			code = 10000,
			msg = string.Empty,
			data = default(object)
		})
		{
		}

		public ApiResult(string msg = "", int code = 10000) : base(new
		{
			success = true,
			code,
			msg,
			data = default(object)
		})
		{
		}

		public ApiResult(dynamic value, string msg = "", int code = 10000) : base(new
		{
			success = true,
			code,
			msg,
			data = value
		})
		{
		}
	}
}