using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MSFramework.Domain
{
	public abstract class Enumeration<TId> : IComparable
		where TId : IComparable
	{
		public string Name { get; private set; }

		public TId Id { get; private set; }

		protected Enumeration(TId id, string name)
		{
			Id = id;
			Name = name;
		}

		public override string ToString() => Name;
		
		public static IEnumerable<T> GetAll<T>() where T : Enumeration<TId>
		{
			var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

			return fields.Select(f => f.GetValue(null)).Cast<T>();
		}

		public override bool Equals(object obj)
		{
			var otherValue = obj as Enumeration<TId>;

			if (otherValue == null)
				return false;

			var typeMatches = GetType() == obj.GetType();
			var valueMatches = Id.Equals(otherValue.Id);

			return typeMatches && valueMatches;
		}

		// ReSharper disable once NonReadonlyMemberInGetHashCode
		public override int GetHashCode() => Id.GetHashCode();

		public int CompareTo(object other) => Id.CompareTo(((Enumeration) other).Id);

		public static T FromId<T>(TId value) where T : Enumeration<TId>
		{
			var matchingItem = Parse<T, TId>(value, "value",
				item => item.Id.Equals(value));
			return matchingItem;
		}

		public static T FromName<T>(string name) where T : Enumeration<TId>
		{
			var matchingItem = Parse<T, string>(name, "display name", item => item.Name == name);
			return matchingItem;
		}

		private static T Parse<T, TK>(TK value, string description, Func<T, bool> predicate) where T : Enumeration<TId>
		{
			var matchingItem = GetAll<T>().FirstOrDefault(predicate);

			if (matchingItem == null)
				throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");

			return matchingItem;
		}
	}

	public abstract class Enumeration : Enumeration<int>
	{
		protected Enumeration(int id, string name) : base(id, name)
		{
		}
	}
}