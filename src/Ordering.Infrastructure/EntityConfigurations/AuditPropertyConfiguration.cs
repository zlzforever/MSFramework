using MSFramework.Audit;
using MSFramework.Ef;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditPropertyConfiguration
		: EntityTypeConfigurationBase<AuditedProperty, OrderingContext>
	{
	}
}