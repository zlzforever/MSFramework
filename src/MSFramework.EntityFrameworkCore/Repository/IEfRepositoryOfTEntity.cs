using MSFramework.Domain;

namespace MSFramework.EntityFrameworkCore.Repository
{
	/// <summary>
	/// A shortcut of <see cref="IRepository{TEntity,TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public interface IEfRepository<TEntity> : IEfRepository<TEntity, int> where TEntity : class, IEntity<int>
	{
	}
}