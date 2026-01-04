using MicroserviceFramework;

namespace Ordering.API;

public class TestInitializerBase : InitializerBase
{
    public TestInitializerBase()
    {
        Order = 1;
    }

    public override void Start()
    {
    }
}
