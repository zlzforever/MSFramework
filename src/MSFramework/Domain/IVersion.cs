using System.ComponentModel.DataAnnotations;

namespace MSFramework.Domain
{
	public interface IVersion
	{
		/// <summary>
		/// 数据版本号，
		/// </summary>
		[StringLength(50)]
		string ConcurrencyStamp { get; set; }
	}
}