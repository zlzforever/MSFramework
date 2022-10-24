using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Project
{
	public class Queries
	{
		public static class V10
		{
			public class GetProductByIdQuery : IRequest<Dtos.V10.ProductOut>
			{
				public ObjectId Id { get; set; }
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