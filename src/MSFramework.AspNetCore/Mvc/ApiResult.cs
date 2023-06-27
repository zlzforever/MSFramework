using System.Text.Json;

namespace MicroserviceFramework.AspNetCore.Mvc;

public class ApiResult<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// 业务代码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Msg { get; set; } = string.Empty;

    public T Data { get; set; }

    public ApiResult() : this(default)
    {
    }

    public ApiResult(T data)
    {
        Data = data;
    }

    public override string ToString()
    {
        return $"Code: {Code}, Success: {Success}, Msg: {Msg}";
    }
}

public class ApiResult : ApiResult<object>
{
    public static readonly ApiResult Ok = new(null) { Code = 0, Success = true, Msg = "" };

    public static readonly ApiResult Error = new(null) { Code = 1, Success = false, Msg = "服务器内部错误" };

    public ApiResult() : this(null)
    {
    }

    public ApiResult(object data) : base(data)
    {
    }

    public override string ToString()
    {
        return $"Code: {Code}, Success: {Success}, Msg: {Msg}, Data: {JsonSerializer.Serialize(Data)}";
    }
}
