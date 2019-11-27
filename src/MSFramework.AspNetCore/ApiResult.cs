using Microsoft.AspNetCore.Mvc;

namespace MSFramework.AspNetCore
{
	public class ApiResult : JsonResult
	{
		public static int SuccessCode = 0;

		public ApiResult(object value) : base(value)
		{
		}

		public ApiResult() : base(new
		{
			success = true,
			code = SuccessCode,
			msg = string.Empty,
			data = default(object)
		})
		{
		}

		public ApiResult(string msg = "", int code = default) : base(new
		{
			success = true,
			code = code == default ? SuccessCode : code,
			msg,
			data = default(object)
		})
		{
		}

		public ApiResult(dynamic value, string msg = "", int code = default) : base(new
		{
			success = true,
			code = code == default ? SuccessCode : code,
			msg,
			data = value
		})
		{
		}
	}
}