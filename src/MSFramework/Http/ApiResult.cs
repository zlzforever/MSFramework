using System;
using System.Collections.Generic;
using MSFramework.Data;

namespace MSFramework.Http
{
	public interface IApiResult
	{
	}

	/// <summary>
	/// 用于内部系统调用外部 API 的返回做返序列化
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ApiResult<T> : IApiResult where T : class
	{
		public bool Success { get; set; } = true;
		public int Code { get; set; }
		public string Msg { get; set; }
		public T Data { get; set; }
	}

	/// <summary>
	/// 用于内部系统返回到外部
	/// </summary>
	public class ApiResult : ApiResult<object>
	{
		public ApiResult(bool success = true, object data = null, string msg = "", int code = 0)
		{
			Success = success;
			Data = data;
			Msg = msg;
			Code = code;
			if (Success && code != 0)
			{
				throw new ArgumentException("0 is the success code");
			}

			if (!Success && code == 0)
			{
				throw new ArgumentException("0 is the success code");
			}
		}

		public ApiResult(object data = null, string msg = "", int code = 0)
			: this(true, data, msg, code)
		{
		}

		public ApiResult()
			: this(null, "", 0)
		{
		}
	}

	public class ErrorApiResult : ApiResult
	{
		public ErrorApiResult(string msg, int code = 1) : base(false, null, msg, code)
		{
		}
	}

	public class LayUIApiPagedQueryResult<TEntity> : PagedQueryResult<TEntity>, IApiResult
	{
		public bool Success => true;
		public int Code => 0;

		private LayUIApiPagedQueryResult()
		{
		}

		public LayUIApiPagedQueryResult(int page, int limit, int count, List<TEntity> data)
		{
			Page = page;
			Limit = limit;
			Count = count;
			Data = data;
		}
	}

	public class LayUIApiPagedQueryResult : LayUIApiPagedQueryResult<object>
	{
		public LayUIApiPagedQueryResult(int page, int limit, int count, List<object> data)
			: base(page, limit, count, data)
		{
		}
	}
}