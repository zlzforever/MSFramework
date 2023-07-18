using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MicroserviceFramework.Linq.Expression;
using Xunit;

namespace MSFramework.Tests;

public class LinqExpressionTests
{
    [Fact]
    public void Or()
    {
        var list = new List<int>
        {
            1,
            2,
            3,
            4,
            5
        };
        Expression<Func<int, bool>> predicate = x => x == 1;
        var p2 = predicate.Or(x => x == 2);

        var q = list.Where(p2.Compile()).ToList();
        Assert.Equal(1, q[0]);
        Assert.Equal(2, q[1]);
    }
}
