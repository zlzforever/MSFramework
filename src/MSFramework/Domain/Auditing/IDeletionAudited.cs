using System;

namespace MSFramework.Domain.Auditing
{
    /// <summary>
    /// This interface can be implemented to store deletion information (who delete and when deleted).
    /// </summary>
    public interface IDeletionAudited : IHasDeletionTime
    {
        /// <summary>
        /// Id of the deleter user.
        /// </summary>
        string DeleterUserId { get; set; }
    }
}