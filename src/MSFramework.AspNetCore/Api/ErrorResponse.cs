namespace MSFramework.AspNetCore.Api
{
	public class ErrorResponse : Response
	{
		public ErrorResponse(string msg, int code = 1) : base(null, msg, false, code)
		{
		}
	}
}