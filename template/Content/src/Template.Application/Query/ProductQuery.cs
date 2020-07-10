using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Common;
using MSFramework.Extensions;
using MSFramework.Mapper;
using Template.Application.DTO;
using Template.Domain.AggregateRoot;
using Template.Infrastructure;

namespace Template.Application.Query
{
	/// <summary>
	/// 可以直接使用 Ef，若对性能有高要求则使用 Dapper 即可
	/// </summary>
	public class ProductQuery : IProductQuery
	{
		private readonly AppDbContext _dbContext;
		private readonly IObjectMapper _mapper;

		public ProductQuery(
			AppDbContext dbContext,
			IObjectMapper mapper)
		{
			_dbContext = dbContext;
			_mapper = mapper;
		}

		public async Task<ProductOut> GetByNameAsync(string name)
		{
			var product = await _dbContext.Set<Product>().FirstOrDefaultAsync(x => x.Name == name);
			return _mapper.Map<ProductOut>(product);
		}

		public async Task<PagedResult<ProductOut>> PagedQueryAsync(string keyword, int page, int limit)
		{
			keyword = keyword?.Trim();

			PagedResult<Product> result;
			if (string.IsNullOrWhiteSpace(keyword))
			{
				result = await _dbContext.Set<Product>().PagedQueryAsync(page, limit,
					null,
					new OrderCondition<Product, DateTimeOffset?>(x => x.LastModificationTime, true));
			}
			else
			{
				result = await _dbContext.Set<Product>().PagedQueryAsync(page, limit,
					x => x.Name.Contains(keyword),
					new OrderCondition<Product, DateTimeOffset?>(x => x.LastModificationTime, true));
			}

			return _mapper.Map<PagedResult<ProductOut>>(result);
		}
	}
}