using MSFramework.Audit;
using MSFramework.Ef;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditOperationConfiguration
		: EntityTypeConfigurationBase<AuditedOperation, OrderingContext>
	{
	}
}