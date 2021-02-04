namespace MicroserviceFramework.AspNetCore.Mvc
{
	public interface IResponse
	{
	}

	public class Response : Response<object>
	{
		public Response(object data,
			string msg = null,
			bool success = true,
			int code = 0) : base(data, msg, success, code)
		{
		}

		public static readonly Response Ok = new(null);
		public static readonly Response Error = new(null, null, false, 1);
	}

	public class Response<T> : IResponse
	{
		public bool Success { get; private set; }
		public int Code { get; private set; }
		public string Msg { get; private set; }
		public T Data { get; private set; }

		public Response(T data, string msg = null, bool success = true, int code = 0)
		{
			Success = success;
			Code = code;
			Data = data;
			Msg = msg;
		}

		public override string ToString()
		{
			return $"[{GetType().Name}] Success = {Success}, Code = {Code}, Msg = {Msg}, Data = {Data}";
		}
	}
}