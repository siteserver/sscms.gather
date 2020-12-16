using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class TestingContentController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var rule = await _ruleRepository.GetAsync(request.RuleId);

            var regexContentExclude = GatherUtils.GetRegexString(rule.ContentExclude);
            var regexChannel = GatherUtils.GetRegexChannel(rule.ContentChannelStart, rule.ContentChannelEnd);
            var regexContent = GatherUtils.GetRegexContent(rule.ContentContentStart, rule.ContentContentEnd);
            var regexContent2 = string.Empty;
            if (!string.IsNullOrEmpty(rule.ContentContentStart2) && !string.IsNullOrEmpty(rule.ContentContentEnd2))
            {
                regexContent2 = GatherUtils.GetRegexContent(rule.ContentContentStart2, rule.ContentContentEnd2);
            }
            var regexContent3 = string.Empty;
            if (!string.IsNullOrEmpty(rule.ContentContentStart3) && !string.IsNullOrEmpty(rule.ContentContentEnd3))
            {
                regexContent3 = GatherUtils.GetRegexContent(rule.ContentContentStart3, rule.ContentContentEnd3);
            }
            var regexNextPage = GatherUtils.GetRegexUrl(rule.ContentNextPageStart, rule.ContentNextPageEnd);
            var regexTitle = GatherUtils.GetRegexTitle(rule.ContentTitleStart, rule.ContentTitleEnd);
            var contentAttributes = ListUtils.GetStringList(rule.ContentAttributes);

            var attributes = GatherUtils.GetContentNameValueCollection(rule.Charset, request.ContentUrl, rule.CookieString, regexContentExclude, rule.ContentHtmlClearCollection, rule.ContentHtmlClearTagCollection, regexTitle, regexContent, regexContent2, regexContent3, regexNextPage, regexChannel, contentAttributes, rule);

            var list = new List<KeyValuePair<string, string>>();

            //var contentAttributes = Context.ContentApi.GetInputStyles(siteId, rule.ChannelId);

            foreach (var attributeName in attributes.AllKeys)
            {
                var value = attributes[attributeName];

                if (string.IsNullOrEmpty(value)) continue;
                if (StringUtils.EqualsIgnoreCase(nameof(SSCMS.Models.Content.ImageUrl), attributeName))
                {
                    var imageUrl = GatherUtils.GetUrlByBaseUrl(value, request.ContentUrl);
                    list.Add(new KeyValuePair<string, string>(attributeName, imageUrl));
                }
                else
                {
                    list.Add(new KeyValuePair<string, string>(attributeName, value));
                }
            }

            return new GetResult
            {
                Attributes = list
            };
        }
    }
}
