using System;
using System.Threading.Tasks;
using SSCMS.Gather.Abstractions;
using SSCMS.Gather.Models;
using SSCMS.Plugins;
using SSCMS.Repositories;
using SSCMS.Utils;

namespace SSCMS.Gather.Core
{
    public class GatherTask : IPluginScheduledTask
    {
        private readonly IGatherManager _gatherManager;
        private readonly IChannelRepository _channelRepository;
        private readonly IRuleRepository _ruleRepository;

        public GatherTask(IGatherManager gatherManager, IChannelRepository channelRepository, IRuleRepository ruleRepository)
        {
            _gatherManager = gatherManager;
            _channelRepository = channelRepository;
            _ruleRepository = ruleRepository;
        }

        public string TaskType => GatherManager.TaskType;

        public async Task ExecuteAsync(string settings)
        {
            var config = TranslateUtils.JsonDeserialize<GatherTaskSettings>(settings);
            if (config != null && config.RuleIds != null)
            {
                foreach (var ruleId in config.RuleIds)
                {
                    var rule = await _ruleRepository.GetAsync(ruleId);
                    if (rule == null) continue;

                    var channel = await _channelRepository.GetAsync(rule.ChannelId);

                    if (channel == null || channel.SiteId != config.SiteId)
                    {
                        throw new Exception("采集错误，请设置需要采集到的栏目！");
                    }
                    else
                    {
                        await _gatherManager.GatherChannelsAsync(0, config.SiteId, ruleId, StringUtils.Guid());
                    }
                }
            }
        }
    }
}
