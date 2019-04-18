namespace MSFramework.Domain.Entity
{
	public interface IHasConcurrencyStamp
	{
		string ConcurrencyStamp { get; set; }
	}
}