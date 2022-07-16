using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicroserviceFramework.Domain
{
	public abstract class Enumeration : IComparable
	{
		public string Name { get; private set; }

		public string Id { get; private set; }

		protected Enumeration(string id, string name)
		{
			Id = id;
			Name = name;
		}

		public override string ToString() => Id;

		public static IEnumerable<T> GetAll<T>() where T : Enumeration
		{
			var enumerations = GetAll(typeof(T));
			return enumerations.Cast<T>();
		}

		public static IEnumerable<Enumeration> GetAll(Type type)
		{
			return type
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
				.Where(i => i.FieldType.IsSubclassOf(typeof(Enumeration))).Select(f => (Enumeration)f.GetValue(null));
		}

		public override bool Equals(object obj)
		{
			var otherValue = obj as Enumeration;

			if (otherValue == null)
				return false;

			var typeMatches = GetType() == obj.GetType();
			var valueMatches = Id.Equals(otherValue.Id);

			return typeMatches && valueMatches;
		}

		public override int GetHashCode() => Id.GetHashCode();

		public static T FromValue<T>(string value) where T : Enumeration
		{
			var matchingItem = Parse<T, string>(value, "value", item => item.Id == value);
			return matchingItem;
		}

		public static T FromDisplayName<T>(string displayName) where T : Enumeration
		{
			var matchingItem = Parse<T, string>(displayName, "display name", item => item.Name == displayName);
			return matchingItem;
		}

		private static T Parse<T, TK>(TK value, string description, Func<T, bool> predicate) where T : Enumeration
		{
			var matchingItem = GetAll<T>().FirstOrDefault(predicate);

			if (matchingItem == null)
				throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");

			return matchingItem;
		}

		public static Enumeration Parse(Type type, string value)
		{
			var matchingItem = GetAll(type).FirstOrDefault(x => x.Id == value);

			if (matchingItem == null)
			{
				throw new InvalidOperationException($"'{value}' is not a valid in {type}");
			}

			return matchingItem;
		}

		public int CompareTo(object other) => string.Compare(Id, ((Enumeration)other).Id, StringComparison.Ordinal);
	}
}