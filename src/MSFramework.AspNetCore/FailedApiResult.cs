namespace MSFramework.AspNetCore
{
	public class FailedApiResult : ApiResult
	{
		public FailedApiResult(string msg = "Internal Error", int code = 20000) : base(new
		{
			success = false,
			code,
			msg,
			data = default(object)
		})
		{
			if (code < 20000 || code >= 30000)
			{
				throw new MSFrameworkException("Failed code should be greater than 20000 and less than 30000");
			}

			StatusCode = 500;
		}
	}
}