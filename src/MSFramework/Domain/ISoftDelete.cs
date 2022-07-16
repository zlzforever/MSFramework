namespace MicroserviceFramework.Domain
{
    public interface ISoftDelete
    {
        /// <summary>
        /// 是否已经删除
        /// </summary>
        bool IsDeleted { get; }
    }
}