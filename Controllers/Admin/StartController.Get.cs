using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class StartController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var rule = await _ruleRepository.GetAsync(request.RuleId);

            var site = await _siteRepository.GetAsync(request.SiteId);
            var channels = await _channelRepository.GetCascadeChildrenAsync(site, site.Id,
                async summary =>
                {
                    var count = await _contentRepository.GetCountAsync(site, summary);
                    return new
                    {
                        Count = count
                    };
                });

            var channel = await _channelRepository.GetAsync(rule.ChannelId);
            var channelIds = new List<int>();
            if (channel != null)
            {
                channelIds = ListUtils.GetIntList(channel.ParentsPath);
                channelIds.Add(rule.ChannelId);
                channelIds.Remove(site.Id);
            }

            return new GetResult
            {
                Rule = rule,
                Channels = channels,
                ChannelIds = channelIds
            };
        }
    }
}
