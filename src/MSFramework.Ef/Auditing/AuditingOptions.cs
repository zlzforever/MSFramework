using MicroserviceFramework.Extensions.Options;

namespace MicroserviceFramework.Ef.Auditing;

[OptionsType]
public class AuditingOptions
{
    public string AuditingDbContextTypeName { get; set; }
}
