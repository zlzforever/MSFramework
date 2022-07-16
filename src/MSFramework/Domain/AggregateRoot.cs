using System;

namespace MicroserviceFramework.Domain
{
    [Serializable]
    public abstract class AggregateRoot : EntityBase,
        IAggregateRoot
    {
    }

    [Serializable]
    public abstract class AggregateRoot<TKey> :
        EntityBase<TKey>,
        IAggregateRoot<TKey> where TKey : IEquatable<TKey>
    {
        protected AggregateRoot(TKey id) : base(id)
        {
        }
    }
}