using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.ObjectMapper;
using Microsoft.EntityFrameworkCore;
using Template.Application.Project.DTOs;
using Template.Domain.Aggregates.Project;
using Template.Infrastructure;

namespace Template.Application.Project.Queries
{
	public class GetProductByNameQueryHandler : IQueryHandler<GetProductByNameQuery, ProductOut>
	{
		private readonly TemplateDbContext _dbContext;
		private readonly IObjMapper _mapper;

		public GetProductByNameQueryHandler(
			TemplateDbContext dbContext,
			IObjMapper mapper)
		{
			_dbContext = dbContext;
			_mapper = mapper;
		}

		public async Task<ProductOut> HandleAsync(GetProductByNameQuery query,
			CancellationToken cancellationToken = new CancellationToken())
		{
			var product = await _dbContext.Set<Product>()
				.FirstOrDefaultAsync(x => x.Name == query.Name, cancellationToken: cancellationToken);
			return _mapper.Map<ProductOut>(product);
		}
	}
}