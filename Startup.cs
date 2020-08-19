using Microsoft.Extensions.DependencyInjection;
using SSCMS.Gather.Abstractions;
using SSCMS.Gather.Core;
using SSCMS.Plugins;

namespace SSCMS.Gather
{
    public class Startup : IPluginConfigureServices
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRuleRepository, RuleRepository>();
            services.AddScoped<IGatherManager, GatherManager>();
        }
    }
}
