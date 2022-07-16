using System;

namespace MicroserviceFramework.Domain
{
    public interface IModification
    {
        /// <summary>
        /// Last modifier user for this entity.
        /// </summary>
        string LastModifierId { get; }

        /// <summary>
        /// The last modified time for this entity.
        /// </summary>
        DateTimeOffset? LastModificationTime { get; }

        void SetModification(string userId, DateTimeOffset modificationTime = default);
    }
}