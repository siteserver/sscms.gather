using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Gather.Core;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class StartController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var rule = await _ruleRepository.GetAsync(request.RuleId);

            rule.ChannelId = request.ChannelId;
            rule.GatherNum = request.GatherNum;
            rule.IsChecked = request.IsChecked;
            rule.GatherUrlIsCollection = request.GatherUrlIsCollection;
            rule.GatherUrlIsSerialize = request.GatherUrlIsSerialize;
            rule.GatherUrlCollection = request.GatherUrlCollection;
            rule.GatherUrlSerialize = request.GatherUrlSerialize;
            rule.SerializeFrom = request.SerializeFrom;
            rule.SerializeTo = request.SerializeTo;
            rule.SerializeInterval = request.SerializeInterval;
            rule.SerializeIsOrderByDesc = request.SerializeIsOrderByDesc;
            rule.SerializeIsAddZero = request.SerializeIsAddZero;
            rule.LastGatherDate = DateTime.Now;

            await _ruleRepository.UpdateAsync(rule);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
