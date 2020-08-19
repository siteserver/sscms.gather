using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;
using SSCMS.Gather.Models;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class AddController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsAdd))
            {
                return Unauthorized();
            }

            Rule rule;
            List<string> contentHtmlClearList;
            List<string> contentHtmlClearTagList;
            Dictionary<string, string> attributesDict;
            if (request.RuleId > 0)
            {
                rule = await _ruleRepository.GetAsync(request.RuleId);
                contentHtmlClearList = ListUtils.GetStringList(rule.ContentHtmlClearCollection);
                contentHtmlClearTagList = ListUtils.GetStringList(rule.ContentHtmlClearTagCollection);
                attributesDict = TranslateUtils.JsonDeserialize<Dictionary<string, string>>(rule.ContentAttributesXml);
            }
            else
            {
                rule = new Rule
                {
                    SiteId = request.SiteId,
                    Charset = Charset.Utf8,
                    IsSaveImage = true,
                    IsSetFirstImageAsImageUrl = true,
                    IsOrderByDesc = true,
                    GatherUrlIsCollection = true,
                    ContentHtmlClearCollection = "",
                    ContentHtmlClearTagCollection = ""
                };
                contentHtmlClearList = new List<string>
                    {
                        "script",
                        "object",
                        "iframe"
                    };
                contentHtmlClearTagList = new List<string>
                    {
                        "font",
                        "div",
                        "span"
                    };
                attributesDict = new Dictionary<string, string>();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            var channels = await _channelRepository.GetCascadeChildrenAsync(site, site.Id,
                async summary =>
                {
                    var count = await _contentRepository.GetCountAsync(site, summary);
                    return new
                    {
                        Count = count
                    };
                });

            var charsetList = ListUtils.GetSelects<Charset>();

            return new GetResult
            {
                Rule = rule,
                Channels = channels,
                CharsetList = charsetList,
                ContentHtmlClearList = contentHtmlClearList,
                ContentHtmlClearTagList = contentHtmlClearTagList,
                AttributesDict = attributesDict
            };
        }
    }
}
