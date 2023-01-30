using System.Text.Json;

namespace MicroserviceFramework.AspNetCore.Mvc;

public class ApiResult
{
    public static readonly ApiResult Ok = new() { Code = 0, Success = true, Msg = "", Data = null };

    public static readonly ApiResult Error = new() { Code = 1, Success = false, Msg = "服务器内部错误", Data = null };

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

    /// <summary>
    /// 数据
    /// </summary>
    public object Data { get; set; }

    public ApiResult()
    {
    }

    public ApiResult(object data) : this()
    {
        Data = data;
    }

    public override string ToString()
    {
        return $"Code: {Code}, Success: {Success}, Msg: {Msg}, Data: {JsonSerializer.Serialize(Data)}";
    }
}
