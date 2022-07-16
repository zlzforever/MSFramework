using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.AspNetCore.AccessControl
{
    public class ApiInfoInitializer : InitializerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public ApiInfoInitializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await new Cerberus.AspNetCore.AccessControl.ApiInfoInitializer().InitializeAsync(_serviceProvider);
        }
    }
}