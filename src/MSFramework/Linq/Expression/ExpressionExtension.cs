using System;
using System.Linq.Expressions;

namespace MicroserviceFramework.Linq.Expression;

/// <summary>
///
/// </summary>
public static class ExpressionExtension
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="expr1"></param>
    /// <param name="expr2"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = System.Linq.Expressions.Expression.Invoke(expr2, expr1.Parameters);
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>
            (System.Linq.Expressions.Expression.Or(expr1.Body, invokedExpr), expr1.Parameters);
    }
}
