using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Aggregates.Project;
using Template.Infrastructure;

namespace Template.Application.Project.QueryHandlers
{
	public class GetProductByIdQueryHandler : IRequestHandler<Queries.V10.GetProductByIdQuery, Dto.V10.ProductOut>
	{
		private readonly TemplateDbContext _dbContext;
		private readonly IObjectAssembler _objectAssembler;

		public GetProductByIdQueryHandler(
			TemplateDbContext dbContext,
			IObjectAssembler objectAssembler)
		{
			_dbContext = dbContext;
			_objectAssembler = objectAssembler;
		}

		public async Task<Dto.V10.ProductOut> HandleAsync(Queries.V10.GetProductByIdQuery query,
			CancellationToken cancellationToken = new CancellationToken())
		{
			var product = await _dbContext.Set<Product>()
				.FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken: cancellationToken);
			return _objectAssembler.To<Dto.V10.ProductOut>(product);
		}
	}
}