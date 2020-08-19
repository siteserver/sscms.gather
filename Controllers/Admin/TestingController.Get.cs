using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class TestingController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var rule = await _ruleRepository.GetAsync(request.RuleId);
            var gatherUrls = GatherUtils.GetGatherUrlList(rule);

            return new GetResult
            {
                Rule = rule,
                GatherUrls = gatherUrls
            };
        }
    }
}
