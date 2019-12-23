namespace MSFramework.AspNetCore
{
	public class FailedApiResult : ApiResult
	{
		public FailedApiResult(string msg = "Internal Error", int code = 110, int? statusCode = 500) : base(new
		{
			success = false,
			code,
			msg,
			data = default(object)
		})
		{
			StatusCode = statusCode;
		}
	}
}