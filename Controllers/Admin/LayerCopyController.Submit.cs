using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Extensions;
using SSCMS.Gather.Core;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class LayerCopyController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            if (await _ruleRepository.IsExistsAsync(request.SiteId, request.RuleName))
            {
                return this.Error("复制失败，已存在相同名称的采集规则！");
            }

            var rule = await _ruleRepository.GetAsync(request.RuleId);
            rule.RuleName = request.RuleName;
            rule.LastGatherDate = null;

            await _ruleRepository.InsertAsync(rule);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
