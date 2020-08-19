using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Gather.Core;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class StartController
    {
        [HttpPost, Route(RouteActionsGather)]
        public async Task<ActionResult<BoolResult>> Gather([FromBody] GatherRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            //Task.Run(async () => await Main.GatherRuleRepository.GatherListAsync(adminInfo, siteId, ruleId, guid, false)).ConfigureAwait(false).GetAwaiter();
            _gatherManager.Start(_authManager.AdminId, request.SiteId, request.RuleId, request.Guid, false);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
