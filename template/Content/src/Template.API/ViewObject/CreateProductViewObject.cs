using System.ComponentModel.DataAnnotations;

namespace Template.API.ViewObject
{
	public class CreateProductViewObject
	{
		/// <summary>
		/// Name
		/// </summary>
		[Required]
		[StringLength(200)]
		public string Name { get; set; }

		/// <summary>
		/// 价可笑
		/// </summary>
		[Required]
		public int Price { get; set; }

		/// <summary>
		/// 类型
		/// </summary>
		[Required]
		public string Type { get; set; }
	}
}