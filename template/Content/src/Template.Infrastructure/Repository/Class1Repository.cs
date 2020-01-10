using MSFramework.Ef;
using Template.Domain;
using Template.Domain.AggregateRoot;
using Template.Domain.Repository;

namespace Template.Infrastructure.Repository
{
	public class Class1Repository
		: EfRepository<Class1>, IClass1Repository
	{
		public Class1Repository(DbContextFactory context) : base(context)
		{
		}
	}
}