using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.Shared;
using Template.Application.Project.DTOs;

namespace Template.Application.Project.Queries
{
	public class PagedProductQuery : IQuery<PagedResult<ProductOut>>
	{
		public int Page { get; set; }
		public int Limit { get; set; }
		public string Keyword { get; set; }
	}
}