using System.IO;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MSFramework.AspNetCore.Test;

public class FormFileTests : BaseTest

{
    [Fact]
    public async Task SaveFile()
    {
        await using var stream = File.OpenRead("1.csv");
        var formFile = new FormFile(stream, 0, stream.Length, "1", "1.csv");
        await formFile.SaveAsync();
    }
}
