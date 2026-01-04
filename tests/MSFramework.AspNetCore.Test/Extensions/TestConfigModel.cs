using MicroserviceFramework.Extensions.Options;

namespace MSFramework.AspNetCore.Test.Extensions;

[AutoOptions(Section = "TestConfigModel")]
public class TestConfigModel
{
    public string Name { get; set; }

    public int Height { get; set; }
}
