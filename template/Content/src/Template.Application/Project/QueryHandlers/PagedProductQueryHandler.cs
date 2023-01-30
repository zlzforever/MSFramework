using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Linq.Expression;
using MicroserviceFramework.Mediator;
using Template.Domain.Aggregates.Project;
using Template.Infrastructure;

namespace Template.Application.Project.QueryHandlers
{
	public class
		PagedProductQueryHandler : IRequestHandler<Queries.V10.PagedProductQuery, PagedResult<Dto.V10.ProductOut>>
	{
		private readonly TemplateDbContext _dbContext;
		private readonly IObjectAssembler _objectAssembler;

		public PagedProductQueryHandler(
			TemplateDbContext dbContext, IObjectAssembler objectAssembler)
		{
			_dbContext = dbContext;
			_objectAssembler = objectAssembler;
		}

		public async Task<PagedResult<Dto.V10.ProductOut>> HandleAsync(Queries.V10.PagedProductQuery query,
			CancellationToken cancellationToken = new CancellationToken())
		{
			query.Keyword = query.Keyword?.Trim();

			PagedResult<Product> result;
			if (string.IsNullOrWhiteSpace(query.Keyword))
			{
				result = await _dbContext.Set<Product>()
					.OrderByDescending(x => x.LastModificationTime)
					.PagedQueryAsync(query.Page, query.Limit);
			}
			else
			{
				result = await _dbContext.Set<Product>()
					.Where(x => x.Name.Contains(query.Keyword))
					.OrderByDescending(x => x.LastModificationTime)
					.PagedQueryAsync(query.Page, query.Limit);
			}

			return new PagedResult<Dto.V10.ProductOut>(result.Page, result.Limit, result.Total,
				_objectAssembler.To<IEnumerable<Dto.V10.ProductOut>>(result.Data));
		}
	}
}