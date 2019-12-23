using System;
using System.Linq.Expressions;

namespace MSFramework.Data
{
	public class OrderCondition<TEntity, TKey>
	{
		public Expression<Func<TEntity, TKey>> Expression { get; }
		public bool Desc { get; }

		public OrderCondition(Expression<Func<TEntity, TKey>> expression, bool desc = false)
		{
			Expression = expression ?? throw new ArgumentNullException(nameof(expression));
			Desc = desc;
		}
	}
}