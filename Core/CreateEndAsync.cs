using System.Threading.Tasks;
using SSCMS.Gather.Abstractions;
using SSCMS.Parse;
using SSCMS.Plugins;
using SSCMS.Services;

namespace SSCMS.Gather.Core
{
    public class CreateEndAsync : IPluginCreateEndAsync
    {
        private readonly IPathManager _pathManager;
        private readonly IGatherManager _gatherManager;
        private readonly IPlugin _plugin;

        public CreateEndAsync(IPathManager pathManager, IPluginManager pluginManager, IGatherManager gatherManager)
        {
            _pathManager = pathManager;
            _gatherManager = gatherManager;
            _plugin = pluginManager.Current;
        }

        public async Task ParseAsync(IParseContext context)
        {
            var config = await _gatherManager.GetConfigAsync(context.SiteId);
            if (!config.IsEnabled) return;

            var isChannel = false;
            if (config.IsAllChannels)
            {
                isChannel = true;
            }
            else
            {
                if (config.GatherChannels != null && config.GatherChannels.Contains(context.ChannelId))
                {
                    isChannel = true;
                }
            }

            if (!isChannel) return;

            var urlPrefix = _pathManager.GetRootUrl("/assets/gather");
            var apiUrl = _pathManager.GetApiUrl();

            context.HeadCodes[_plugin.PluginId] = $@"
<style>body{{display: none !important}}</style>
<script src=""{urlPrefix}/lib/es6-promise.auto.min.js"" type=""text/javascript""></script>
<script src=""{urlPrefix}/lib/axios-0.18.0.min.js"" type=""text/javascript""></script>
<script src=""{urlPrefix}/lib/sweetalert2-7.28.4.all.min.js"" type=""text/javascript""></script>
<script src=""{urlPrefix}/gather.js"" data-api-url=""{apiUrl}"" data-site-id=""{context.SiteId}"" type=""text/javascript""></script>
";
        }
    }
}
