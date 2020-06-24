namespace MSFramework.AspNetCore.Infrastructure
{
	public class Response
	{
		public bool Success { get; set; }
		public int Code { get; set; }
		public string Msg { get; set; }
		public dynamic Data { get; set; }
	}
}