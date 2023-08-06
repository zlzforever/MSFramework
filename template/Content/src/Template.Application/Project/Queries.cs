using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Project
{
	public class Queries
	{
		public static class V10
		{
			public record GetProductByIdQuery : Request<Dto.V10.ProductOut>
			{
				public ObjectId Id { get; set; }
			}

			public record PagedProductQuery : Request<PagedResult<Dto.V10.ProductOut>>
			{
				public int Page { get; set; }
				public int Limit { get; set; }
				public string Keyword { get; set; }
			}
		}
	}
}