using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;
using System.Threading.Tasks;
using SSCMS.Gather.Models;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class TestingController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<SubmitResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var rule = await _ruleRepository.GetAsync(request.RuleId);

            var regexListArea = GatherUtils.GetRegexArea(rule.ListAreaStart, rule.ListAreaEnd);

            var regexContentUrl = GatherUtils.GetRegexUrl(rule.ContentUrlStart, rule.ContentUrlEnd);
            var regexImageUrl = string.Empty;
            if (rule.ImageSource == ImageSource.List)
            {
                regexImageUrl = GatherUtils.GetRegexUrl(rule.ImageUrlStart, rule.ImageUrlEnd);
            }

            var urls = GatherUtils.GetContentAndImageUrls(request.GatherUrl, rule.Charset, rule.CookieString, regexListArea, regexContentUrl, regexImageUrl);

            return new SubmitResult
            {
                ContentUrls = urls.contentUrls,
                ImageUrls = urls.imageUrls
            };
        }
    }
}
