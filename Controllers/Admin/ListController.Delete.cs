﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class ListController
    {
        [HttpPost, Route(RouteDelete)]
        public async Task<ActionResult<DeleteResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            foreach (var ruleId in request.RuleIds)
            {
              await _ruleRepository.DeleteAsync(ruleId);
            }

            var rules = await _ruleRepository.GetRulesAsync(request.SiteId);
            foreach (var rule in rules)
            {
                var gatherUrlList = GatherUtils.GetGatherUrlList(rule);
                if (gatherUrlList != null && gatherUrlList.Count > 0)
                {
                    var url = gatherUrlList[0];
                    rule.Set("gatherUrl", url);
                }
            }

            return new DeleteResult
            {
                Rules = rules
            };
        }
    }
}
