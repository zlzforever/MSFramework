using System.Text.Json;

namespace MicroserviceFramework.AspNetCore.Mvc;

internal class ApiResultWithErrors : ApiResult
{
    public object Errors { get; set; }

    public override string ToString()
    {
        return
            $"Code: {Code}, Success: {Success}, Msg: {Msg}, Errors: {JsonSerializer.Serialize(Errors)}";
    }
}
