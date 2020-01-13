namespace MSFramework.Http
{
	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public int Code { get; set; }
		public string Msg { get; set; }
		public T Data { get; set; }
		public int Count { get; set; }
		public int Page { get; set; }
		public int Limit { get; set; }
	}

	public class ApiResponse
	{
		public bool Success { get; set; } = true;
		public int Code { get; set; }
		public string Msg { get; set; }
		public dynamic Data { get; set; }
	}

	public class PagedApiResponse
	{
		public bool Success { get; set; }
		public int Code { get; set; }
		public string Msg { get; set; }
		public dynamic Data { get; set; }
		public int Count { get; set; }
		public int Page { get; set; }
		public int Limit { get; set; }
	}
}