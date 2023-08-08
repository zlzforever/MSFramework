using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Domain;

public abstract record ValueObject
{
    public ValueObject Copy()
    {
        return this with { };
    }

    // protected static bool EqualOperator(ValueObject left, ValueObject right)
    // {
    //     if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
    //     {
    //         return false;
    //     }
    //
    //     return ReferenceEquals(left, null) || left.Equals(right);
    // }
    //
    // protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    // {
    //     return !(EqualOperator(left, right));
    // }
    //
    // protected abstract IEnumerable<object> GetEqualityComponents();
    //
    // public override string ToString()
    // {
    //
    //     return $"{GetType().Name}: {string.Join(", ", GetEqualityComponents())}";
    // }
    //
    // public static bool operator ==(ValueObject b, ValueObject c)
    // {
    //     return EqualOperator(b, c);
    // }
    //
    // public static bool operator !=(ValueObject b, ValueObject c)
    // {
    //     return !(b == c);
    // }
    //
    // public override bool Equals(object obj)
    // {
    //     if (obj == null || obj.GetType() != GetType())
    //     {
    //         return false;
    //     }
    //
    //     var other = (ValueObject)obj;
    //
    //     return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    // }
    //
    // public override int GetHashCode()
    // {
    //     return GetEqualityComponents()
    //         .Select(x => x != null ? x.GetHashCode() : 0)
    //         .Aggregate((x, y) => x ^ y);
    // }
    //
    // public ValueObject Clone()
    // {
    //     return this.MemberwiseClone() as ValueObject;
    // }
}
