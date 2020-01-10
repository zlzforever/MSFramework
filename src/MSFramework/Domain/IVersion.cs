namespace MSFramework.Domain
{
	public interface IVersion
	{
		/// <summary>
		/// 数据版本号，
		/// </summary>
		string ConcurrencyStamp { get; set; }
	}
}