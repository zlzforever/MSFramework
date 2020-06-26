using Template.Domain.AggregateRoot;

namespace Template.Application.DTO
{
	public class CreateProductIn
	{
		public string Name { get; set; }
		public int Price { get; set; }
		public ProductType Type { get; set; }
	}
}