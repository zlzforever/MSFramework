using System;
using System.Text.Json;

namespace MicroserviceFramework.AspNetCore.Mvc;

internal class ApiResultWithErrors : ApiResult
{
    public static readonly Type ApiResultWithErrorsType = typeof(ApiResultWithErrors);
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
