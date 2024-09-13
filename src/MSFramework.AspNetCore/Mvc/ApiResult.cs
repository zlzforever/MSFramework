using System;
using System.Text.Json;

namespace MicroserviceFramework.AspNetCore.Mvc;

/// <summary>
///
/// </summary>
/// <param name="data"></param>
/// <typeparam name="T"></typeparam>
public class ApiResult<T>(T data)
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

    /// <summary>
    /// 数据对象
    /// </summary>
    public T Data { get; set; } = data;

    /// <summary>
    ///
    /// </summary>
    protected ApiResult() : this(default)
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Code: {Code}, Success: {Success}, Msg: {Msg}, Data: {JsonSerializer.Serialize(Data)}";
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator ApiResult<T>(T value)
    {
        return new ApiResult<T> { Data = value };
    }
}

/// <summary>
///
/// </summary>
public class ApiResult : ApiResult<object>
{
    /// <summary>
    ///
    /// </summary>
    public static readonly Type Type = typeof(ApiResult);
    /// <summary>
    ///
    /// </summary>
    public static readonly Type GenericType = typeof(ApiResult<>);

    /// <summary>
    ///
    /// </summary>
    public static readonly ApiResult Ok = new() { Code = 0, Success = true, Msg = string.Empty, Data = null };

    /// <summary>
    ///
    /// </summary>
    public static readonly ApiResult Error = new() { Code = 1, Success = false, Msg = "服务器内部错误", Data = null };

    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsApiResult(Type type)
    {
        if (type == null)
        {
            return false;
        }

        if (type == Type)
        {
            return true;
        }

        if (type == ApiResultWithErrors.ApiResultWithErrorsType)
        {
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == GenericType)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Code: {Code}, Success: {Success}, Msg: {Msg}, Data: {JsonSerializer.Serialize(Data)}";
    }
}
