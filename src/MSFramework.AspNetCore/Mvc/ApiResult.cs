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

    protected ApiResult() : this(default)
    {
    }

    public ApiResult(T data)
    {
        Data = data;
    }

    public override string ToString()
    {
        return $"Code: {Code}, Success: {Success}, Msg: {Msg}, Data: {JsonSerializer.Serialize(Data)}";
    }

    public static implicit operator ApiResult<T>(T value)
    {
        return new ApiResult<T> { Data = value };
    }
}

public class ApiResult : ApiResult<object>
{
    public static readonly ApiResult Ok = new() { Code = 0, Success = true, Msg = string.Empty, Data = null };

    public static readonly ApiResult Error = new() { Code = 1, Success = false, Msg = "服务器内部错误", Data = null };

    public override string ToString()
    {
        return $"Code: {Code}, Success: {Success}, Msg: {Msg}, Data: {JsonSerializer.Serialize(Data)}";
    }
}
