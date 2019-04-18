namespace MSFramework.Domain.Entity
{
	/// <summary>
	/// Defines an entity with a single primary key with "Id" property.
	/// </summary>
	/// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
	public interface IEntity<TKey>
	{
		/// <summary>
		/// Unique identifier for this entity.
		/// </summary>
		TKey Id { get; }
	}

	public interface IEntity : IEntity<int>
	{
	}
}
