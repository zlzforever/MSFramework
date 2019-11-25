namespace MSFramework.AspNetCore
{
	public class FailedApiResult : ApiResult
	{
		public FailedApiResult(string msg = "Internal Error", int code = 1000) : base(new
		{
			success = false,
			code,
			msg,
			data = default(object)
		})
		{
			if (code < 10000)
			{
				throw new MSFrameworkException("Failed code should be greater than 10000");
			}

			StatusCode = 500;
		}
	}
}