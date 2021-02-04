using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Domain
{
	public abstract class ValueObject
	{
		private static bool EqualOperator(ValueObject left, ValueObject right)
		{
			if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
			{
				return false;
			}

			return ReferenceEquals(left, null) || left.Equals(right);
		}

		public static bool operator ==(ValueObject left, ValueObject right)
		{
			return EqualOperator(left, right);
		}

		public static bool operator !=(ValueObject left, ValueObject right)
		{
			return !EqualOperator(left, right);
		}

		protected abstract IEnumerable<object> GetAtomicValues();

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
			{
				return false;
			}

			var other = (ValueObject) obj;

			return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
		}

		public override int GetHashCode()
		{
			return GetAtomicValues()
				.Select(x => x != null ? x.GetHashCode() : 0)
				.Aggregate((x, y) => x ^ y);
		}

		public ValueObject Clone()
		{
			return MemberwiseClone() as ValueObject;
		}
	}
}