using System;
using System.Text.Json;

namespace MicroserviceFramework.Common;

internal class ApiResultWithErrors : ApiResult
{
    public static readonly Type ApiResultWithErrorsType = typeof(ApiResultWithErrors);

    /// <summary>
    /// 错误信息
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public object Errors { get; set; }

    public ApiResultWithErrors()
    {
        Success = false;
        Code = 1;
    }

    public override string ToString()
    {
        return
            $"Code: {Code}, Success: {Success}, Msg: {Msg}, Errors: {JsonSerializer.Serialize(Errors)}";
    }
}
