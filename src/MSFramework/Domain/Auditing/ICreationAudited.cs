namespace MSFramework.Domain.Auditing
{
    /// <summary>
    /// This interface can be implemented to store creation information (who and when created).
    /// </summary>
    public interface ICreationAudited : IHasCreationTime
    {
		/// <summary>
		/// Id of the creator user of this entity.
		/// </summary>
		string CreatorUserId { get; set; }
	}
}