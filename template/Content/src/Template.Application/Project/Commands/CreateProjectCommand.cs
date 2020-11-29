using MicroserviceFramework.Application.CQRS;
using Template.Application.Project.DTOs;
using Template.Domain.Aggregates.Project;

namespace Template.Application.Project.Commands
{
	public class CreateProjectCommand : ICommand<CreatProductOut>
	{
		public string Name { get; set; }
		public int Price { get; set; }
		public ProductType Type { get; set; }
	}
}