using Microsoft.AspNetCore.Mvc;
using MSFramework.Http;

namespace MSFramework.AspNetCore
{
	public class ApiResult : JsonResult
	{
		private const int SuccessCode = 0;

		public ApiResult() : base(new ApiResponse())
		{
		}

		public ApiResult(string msg = "", int code = SuccessCode) : base(new ApiResponse
		{
			Code = code == default ? SuccessCode : code,
			Msg = msg
		})
		{
		}

		public ApiResult(dynamic value, string msg = "", int code = SuccessCode) : base(new ApiResponse
		{
			Code = code == default ? SuccessCode : code,
			Msg = msg,
			Data = value
		})
		{
		}
	}
}