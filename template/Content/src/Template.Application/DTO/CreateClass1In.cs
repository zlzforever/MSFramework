using System.ComponentModel.DataAnnotations;

namespace Template.Application.DTO
{
	public class CreateClass1In
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }
	}
}