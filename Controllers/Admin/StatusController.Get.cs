using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;
using SSCMS.Gather.Models;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class StatusController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var rules = new List<Rule>();
            foreach (var ruleId in ListUtils.GetIntList(request.RuleIds))
            {
                var rule = await _ruleRepository.GetAsync(ruleId);
                var channel = await _channelRepository.GetAsync(rule.ChannelId);
                
                if (channel == null || channel.SiteId != rule.SiteId)
                {
                    rule.Set("error", "采集错误，请设置需要采集到的栏目！");
                }
                else
                {
                    _gatherManager.Start(_authManager.AdminId, request.SiteId, ruleId, rule.Guid);
                }

                rule.Set("cache", new {});
                rule.Set("percentage", new {});
                rules.Add(rule);
            }

            return new GetResult
            {
                Rules = rules
            };
        }
    }
}
