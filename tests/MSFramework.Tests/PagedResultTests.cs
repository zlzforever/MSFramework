using System;
using MicroserviceFramework.Common;
using Xunit;

namespace MSFramework.Tests;

public class PagedResultTests
{
    [Fact]
    public void ForeachNull()
    {
        var result = new PagedResult<object>(1, 10, 100, null);
        foreach (var item in result)
        {
            Console.WriteLine(item.ToString());
        }
    }
}
