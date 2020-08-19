using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class TestingController
    {
        [HttpPost, Route(RouteActionsGetContentUrls)]
        public async Task<ActionResult<GetContentUrlsResult>> GetContentUrls([FromBody] GetContentUrlsRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var ruleInfo = await _ruleRepository.GetAsync(request.RuleId);

            var regexUrlInclude = GatherUtils.GetRegexString(ruleInfo.UrlInclude);
            var regexListArea = GatherUtils.GetRegexArea(ruleInfo.ListAreaStart, ruleInfo.ListAreaEnd);

            var contentUrls = GatherUtils.GetContentUrls(request.GatherUrl, ruleInfo.Charset, ruleInfo.CookieString, regexListArea, regexUrlInclude);

            return new GetContentUrlsResult
            {
                ContentUrls = contentUrls
            };
        }
    }
}
