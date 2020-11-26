// using System.Threading;
// using System.Threading.Tasks;
// using MicroserviceFramework.Application.CQRS;
// using MicroserviceFramework.Domain;
// using MicroserviceFramework.Shared;
//
// namespace Ordering.Domain.AggregateRoots
// {
// 	public interface IResourceRepository : IRepository<Resource>
// 	{
// 	}
//
// 	public class Resource : ModificationAggregateRoot
// 	{
// 		private Resource() : base(ObjectId.NewId())
// 		{
// 		}
//
// 		public static Resource Create()
// 		{
// 			return new Resource();
// 		}
//
// 		public void Approve()
// 		{
// 			// do check permission
// 			// do approve
// 		}
// 	}
//
// 	public class CreateResourceCommand : ICommand<string>
// 	{
// 	}
//
// 	public class CreateResourceCommandHandler : ICommandHandler<CreateResourceCommand, string>
// 	{
// 		private readonly IResourceRepository _resourceRepository;
//
// 		public CreateResourceCommandHandler(IResourceRepository resourceRepository)
// 		{
// 			_resourceRepository = resourceRepository;
// 		}
//
// 		public async Task<string> HandleAsync(CreateResourceCommand command,
// 			CancellationToken cancellationToken = default)
// 		{
// 			var resource = Resource.Create();
// 			await _resourceRepository.InsertAsync(resource);
// 			return resource.Id.ToString();
// 		}
// 	}
//
// 	public class CreateResourceAndApproveCommand : ICommand
// 	{
// 	}
//
// 	public class CreateResourceAndApproveCommandHandler : ICommandHandler<CreateResourceAndApproveCommand>
// 	{
// 		private readonly IResourceRepository _resourceRepository;
//
// 		public CreateResourceAndApproveCommandHandler(IResourceRepository resourceRepository)
// 		{
// 			_resourceRepository = resourceRepository;
// 		}
//
// 		public async Task HandleAsync(CreateResourceAndApproveCommand command,
// 			CancellationToken cancellationToken = default)
// 		{
// 			var resource = Resource.Create();
// 			resource.Approve();
// 			await _resourceRepository.InsertAsync(resource);
// 		}
// 	}
// }