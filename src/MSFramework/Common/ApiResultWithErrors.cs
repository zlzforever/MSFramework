using System;
using System.Text.Json;

namespace MicroserviceFramework.Common;

/// <summary>
/// 带错误详情的 <see cref="ApiResult"/> 子类，用于返回验证错误、业务错误等场景。
/// </summary>
internal class ApiResultWithErrors : ApiResult
{
    /// <summary>
    /// <see cref="ApiResultWithErrors"/> 的 <see cref="Type"/>，供反射和序列化使用。
    /// </summary>
    public static readonly Type ApiResultWithErrorsType = typeof(ApiResultWithErrors);

    /// <summary>
    /// 错误信息
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public object Errors { get; set; }

    /// <summary>
    /// 默认构造，设置 <see cref="ApiResult.Success"/> = false, <see cref="ApiResult.Code"/> = 1。
    /// </summary>
    public ApiResultWithErrors()
    {
        Success = false;
        Code = 1;
    }

    /// <summary>
    /// 返回调试友好的字符串表示。
    /// </summary>
    public override string ToString()
    {
        return
            $"Code: {Code}, Success: {Success}, Msg: {Msg}, Errors: {JsonSerializer.Serialize(Errors)}";
    }
}
