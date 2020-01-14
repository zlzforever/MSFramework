namespace MSFramework.Http
{
	/// <summary>
	/// 用于内部系统调用外部 API 的返回做返序列化
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ApiResponse<T> where T : class
	{
		public bool Success { get; set; } = true;
		public int Code { get; set; } = 0;
		public string Msg { get; set; }
		public T Data { get; set; }
	}

	/// <summary>
	/// 用于内部系统返回到外部
	/// </summary>
	public class ApiResponse : ApiResponse<object>
	{
	}

	/// <summary>
	/// 用于内部系统调用外部 API 的返回做返序列化
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PagedApiResponse<T> : ApiResponse<T> where T : class
	{
		public int Count { get; set; }
		public int Page { get; set; }
		public int Limit { get; set; }
	}

	/// <summary>
	/// 用于内部系统返回到外部
	/// </summary>
	public class PagedApiResponse : PagedApiResponse<object>
	{
	}
}