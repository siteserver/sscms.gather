using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class StatusController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<SubmitResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var caches = new List<ProgressCache>();
            foreach (var ruleId in ListUtils.GetIntList(request.RuleIds))
            {
                var rule = await _ruleRepository.GetAsync(ruleId);
                var cache = _gatherManager.GetCache(rule.Guid);
                caches.Add(cache);
            }

            return new SubmitResult
            {
                Caches = caches
            };
        }
    }
}
