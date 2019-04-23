using Ordering.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSFramework.Domain.Repository;

namespace Ordering.Infrastructure.Repository
{
	public interface IOrdingRepository : IAggregateRepository<Order, Guid>
	{

	}
}
