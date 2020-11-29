using MicroserviceFramework.Application.CQRS;
using Template.Application.Project.DTOs;

namespace Template.Application.Project.Queries
{
	public class GetProductByNameQuery : IQuery<ProductOut>
	{
		public string Name { get; set; }
	}
}