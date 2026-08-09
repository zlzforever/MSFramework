using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroserviceFramework.Common;

/// <summary>
/// 统一 API 响应结果泛型封装
/// </summary>
/// <param name="data">响应数据</param>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResult<T>(T data)
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// 业务代码
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [JsonPropertyName("msg")]
    public string Msg { get; set; } = string.Empty;

    /// <summary>
    /// 数据对象
    /// </summary>
    [JsonPropertyName("data")]
    public T Data { get; set; } = data;

    /// <summary>
    /// 无参构造函数，用于子类继承或反序列化
    /// </summary>
    protected ApiResult() : this(default)
    {
    }

    /// <summary>
    /// 返回响应结果的字符串表示，包含状态码、消息和数据
    /// </summary>
    /// <returns>格式化后的字符串</returns>
    public override string ToString()
    {
        return $"Code: {Code}, Success: {Success}, Msg: {Msg}, Data: {JsonSerializer.Serialize(Data)}";
    }

    /// <summary>
    /// 将数据对象隐式转换为 <see cref="ApiResult{T}"/>
    /// </summary>
    /// <param name="value">数据对象</param>
    /// <returns>包含数据的响应结果</returns>
    public static implicit operator ApiResult<T>(T value)
    {
        return new ApiResult<T> { Data = value };
    }
}

/// <summary>
/// 无数据类型的 API 响应结果，常用于无返回值的操作
/// </summary>
public class ApiResult : ApiResult<object>
{
    /// <summary>
    /// <see cref="ApiResult"/> 的运行时类型
    /// </summary>
    public static readonly Type Type = typeof(ApiResult);
    /// <summary>
    /// <see cref="ApiResult{T}"/> 的泛型类型定义
    /// </summary>
    public static readonly Type GenericType = typeof(ApiResult<>);

    /// <summary>
    /// 返回一个新的成功响应结果实例。
    /// 每次访问都创建新实例，避免共享可变静态实例被调用方篡改后影响其他请求
    /// </summary>
    public static ApiResult Ok => new() { Code = 0, Success = true, Msg = string.Empty, Data = null };

    /// <summary>
    /// 返回一个新的失败响应结果实例。
    /// 每次访问都创建新实例，避免共享可变静态实例被调用方篡改后影响其他请求
    /// </summary>
    public static ApiResult Error => new() { Code = 1, Success = false, Msg = "服务器内部错误", Data = null };

    /// <summary>
    /// 判断指定类型是否为 API 响应结果类型（含泛型和非泛型）
    /// </summary>
    /// <param name="type">待判断的类型</param>
    /// <returns>如果是 API 响应类型则返回 true</returns>
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
    /// 返回响应结果的字符串表示，包含状态码、消息和数据
    /// </summary>
    /// <returns>格式化后的字符串</returns>
    public override string ToString()
    {
        return $"Code: {Code}, Success: {Success}, Msg: {Msg}, Data: {JsonSerializer.Serialize(Data)}";
    }
}
