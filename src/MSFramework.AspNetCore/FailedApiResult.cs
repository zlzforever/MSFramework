// using MSFramework.Http;
//
// namespace MSFramework.AspNetCore
// {
// 	public class FailedApiResult : ApiResult
// 	{
// 		public FailedApiResult(string msg = "Internal Error", int code = 110, int? statusCode = 500) : base(
// 			new ApiResponse
// 			{
// 				Success = false,
// 				Code = code,
// 				Msg = msg
// 			})
// 		{
// 			StatusCode = statusCode;
// 		}
// 	}
// }