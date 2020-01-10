using System.ComponentModel.DataAnnotations;

namespace Template.API.ViewObject
{
	public class CreateClass1ViewObject
	{
		/// <summary>
		/// Name
		/// </summary>
		[Required]
		[StringLength(200)]
		public string Name { get; set; }
	}
}