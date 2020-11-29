using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.Shared;

namespace Template.Application.Project.Commands
{
	public class DeleteProjectCommand : ICommand
	{
		public ObjectId ProjectId { get; set; }
	}
}