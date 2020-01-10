using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Data;
using MSFramework.Ef;
using Template.Application.DTO;
using Template.Domain;
using Template.Domain.AggregateRoot;

namespace Template.Application.Query
{
	public class Class1Query : IClass1Query
	{
		private readonly EfRepository<Class1> _class1Repository;
		private readonly IMapper _mapper;

		public Class1Query(
			EfRepository<Class1> class1Repository, IMapper mapper)
		{
			_class1Repository = class1Repository;
			_mapper = mapper;
		}

		public async Task<Class1Out> GetClass1ByNameAsync(string name)
		{
			var class1 = await _class1Repository.Entities.FirstOrDefaultAsync(x => x.Name == name);
			return _mapper.Map<Class1Out>(class1);
		}

		public async Task<PagedQueryResult<Class1Out>> PagedQueryAsync(string keyword, int page, int limit)
		{
			keyword = keyword?.Trim();

			PagedQueryResult<Class1> result;
			if (string.IsNullOrWhiteSpace(keyword))
			{
				result = await _class1Repository.PagedQueryAsync(page, limit,
					null,
					new OrderCondition<Class1, DateTimeOffset?>(x => x.LastModificationTime, true));
			}
			else
			{
				result = await _class1Repository.PagedQueryAsync(page, limit,
					x => x.Name.Contains(keyword),
					new OrderCondition<Class1, DateTimeOffset?>(x => x.LastModificationTime, true));
			}

			return new PagedQueryResult<Class1Out>
			{
				Limit = result.Limit,
				Total = result.Total,
				Page = result.Page,
				Entities = _mapper.Map<List<Class1Out>>(result.Entities)
			};
		}
	}
}