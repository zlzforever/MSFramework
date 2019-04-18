namespace MSFramework.Domain.Auditing
{
    /// <summary>
    /// This interface adds <see cref="IDeletionAudited"/> to <see cref="IAudited"/>.
    /// </summary>
    public interface IFullAudited : IAudited, IDeletionAudited
    {
        
    }
}