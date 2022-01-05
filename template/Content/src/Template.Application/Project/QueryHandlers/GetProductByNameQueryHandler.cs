using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Aggregates.Project;
using Template.Infrastructure;

namespace Template.Application.Project.QueryHandlers
{
	public class GetProductByNameQueryHandler : IRequestHandler<Queries.V10.GetProductByNameQuery, Dtos.V10.ProductOut>
	{
		private readonly TemplateDbContext _dbContext;
		private readonly IObjectAssembler _objectAssembler;

		public GetProductByNameQueryHandler(
			TemplateDbContext dbContext,
			IObjectAssembler objectAssembler)
		{
			_dbContext = dbContext;
			_objectAssembler = objectAssembler;
		}

		public async Task<Dtos.V10.ProductOut> HandleAsync(Queries.V10.GetProductByNameQuery query,
			CancellationToken cancellationToken = new CancellationToken())
		{
			var product = await _dbContext.Set<Product>()
				.FirstOrDefaultAsync(x => x.Name == query.Name, cancellationToken: cancellationToken);
			return _objectAssembler.To<Dtos.V10.ProductOut>(product);
		}
	}
}