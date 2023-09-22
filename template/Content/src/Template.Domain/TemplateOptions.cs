using MicroserviceFramework.Extensions.Options;

namespace Template.Domain
{
    [OptionsType]
    public class TemplateOptions
    {
        public string ApiName { get; set; }
        public string Authority { get; set; }
        public bool RequireHttpsMetadata { get; set; }
    }
}
