using MicroserviceFramework.Mediator;
using MongoDB.Bson;
using Template.Domain.Aggregates.Project;

namespace Template.Application.Project
{
	public static class Commands
	{
		public static class V10
		{
			public record CreateProjectCommand : Request<Dto.V10.CreateProductOut>
			{
				public string Name { get; set; }
				public int Price { get; set; }
				public ProductType Type { get; set; }
			}

			public record DeleteProjectCommand : Request<ObjectId>
			{
				public ObjectId ProjectId { get; set; }
			}

			public record ProjectCreatedCommand : Request<ObjectId>
			{
				public ObjectId ProjectId { get; set; }
			}
		}
	}
}