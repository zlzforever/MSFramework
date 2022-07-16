using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain
{
    public abstract class ModificationEntity : ModificationEntity<ObjectId>
    {
        protected ModificationEntity(ObjectId id) : base(id)
        {
        }
    }

    public abstract class ModificationEntity<TKey> : CreationEntity<TKey>, IModification where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Last modifier user for this entity.
        /// </summary>
        public string LastModifierId { get; private set; }

        /// <summary>
        /// The last modified time for this entity.
        /// </summary>
        public DateTimeOffset? LastModificationTime { get; private set; }

        public virtual void SetModification(string userId,
            DateTimeOffset lastModificationTime = default)
        {
            LastModificationTime = lastModificationTime == default ? DateTimeOffset.Now : lastModificationTime;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                LastModifierId = userId;
            }
        }

        protected ModificationEntity(TKey id) : base(id)
        {
        }
    }
}