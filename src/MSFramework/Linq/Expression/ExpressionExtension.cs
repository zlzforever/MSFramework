using System;
using System.Linq.Expressions;

namespace MicroserviceFramework.Linq.Expression;

/// <summary>
/// 表达式树扩展方法
/// </summary>
public static class ExpressionExtension
{
    /// <summary>
    /// 将两个表达式条件进行逻辑或运算
    /// </summary>
    /// <param name="expr1">第一个表达式</param>
    /// <param name="expr2">第二个表达式</param>
    /// <typeparam name="T">表达式参数类型</typeparam>
    /// <returns>逻辑或运算后的表达式</returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = System.Linq.Expressions.Expression.Invoke(expr2, expr1.Parameters);
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>
            (System.Linq.Expressions.Expression.Or(expr1.Body, invokedExpr), expr1.Parameters);
    }
}
