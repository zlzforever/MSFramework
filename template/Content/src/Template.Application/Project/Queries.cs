using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;

namespace Template.Application.Project
{
	public class Queries
	{
		public static class V10
		{
			public class GetProductByNameQuery : IRequest<Dtos.V10.ProductOut>
			{
				public string Name { get; set; }
			}

			public class PagedProductQuery : IRequest<PagedResult<Dtos.V10.ProductOut>>
			{
				public int Page { get; set; }
				public int Limit { get; set; }
				public string Keyword { get; set; }
			}
		}
	}
}