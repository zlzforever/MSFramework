using MicroserviceFramework.Mediator;
using MongoDB.Bson;
using Template.Domain.Aggregates.Project;

namespace Template.Application.Project
{
	public static class Commands
	{
		public static class V10
		{
			public class CreateProjectCommand : IRequest<Dtos.V10.CreatProductOut>
			{
				public string Name { get; set; }
				public int Price { get; set; }
				public ProductType Type { get; set; }
			}

			public class DeleteProjectCommand : IRequest
			{
				public ObjectId ProjectId { get; set; }
			}
		}
	}
}