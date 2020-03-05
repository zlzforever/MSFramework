using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MSFramework.Domain;

namespace MSFramework.Extensions
{
	public static class EnumerationExtensions
	{
		public static T ToEnumeration<T, TId>(this TId value)
			where T : Domain.Enumeration<TId> where TId : IComparable
		{
			return Domain.Enumeration<TId>.FromId<T>(value);
		}

		public static int AbsoluteDifference(this Enumeration firstValue, Enumeration secondValue)
		{
			var absoluteDifference = Math.Abs(firstValue.Id - secondValue.Id);
			return absoluteDifference;
		}
	}
}