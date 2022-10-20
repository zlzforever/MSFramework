using System.Text.Json;

namespace MicroserviceFramework.AspNetCore.Mvc;

public class ApiResultWithErrors : ApiResult
{
    public object Errors { get; set; }

    public override string ToString()
    {
        return
            $"Code: {Code}, Success: {Success}, Msg: {Msg}, Data: {JsonSerializer.Serialize(Data)}, Errors: {JsonSerializer.Serialize(Errors)}";
    }
}
