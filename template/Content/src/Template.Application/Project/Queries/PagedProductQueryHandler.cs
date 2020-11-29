using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.Extensions;
using MicroserviceFramework.ObjectMapper;
using MicroserviceFramework.Shared;
using Template.Application.Project.DTOs;
using Template.Domain.Aggregates.Project;
using Template.Infrastructure;

namespace Template.Application.Project.Queries
{
	public class PagedProductQueryHandler : IQueryHandler<PagedProductQuery, PagedResult<ProductOut>>
	{
		private readonly TemplateDbContext _dbContext;
		private readonly IObjMapper _mapper;

		public PagedProductQueryHandler(
			TemplateDbContext dbContext,
			IObjMapper mapper)
		{
			_dbContext = dbContext;
			_mapper = mapper;
		}

		public async Task<PagedResult<ProductOut>> HandleAsync(PagedProductQuery query,
			CancellationToken cancellationToken = new CancellationToken())
		{
			query.Keyword = query.Keyword?.Trim();

			PagedResult<Product> result;
			if (string.IsNullOrWhiteSpace(query.Keyword))
			{
				result = await _dbContext.Set<Product>().PagedQueryAsync(query.Page, query.Limit,
					null,
					new OrderCondition<Product, DateTimeOffset?>(x => x.ModificationTime, true));
			}
			else
			{
				result = await _dbContext.Set<Product>().PagedQueryAsync(query.Page, query.Limit,
					x => x.Name.Contains(query.Keyword),
					new OrderCondition<Product, DateTimeOffset?>(x => x.ModificationTime, true));
			}

			return _mapper.Map<PagedResult<ProductOut>>(result);
		}
	}
}