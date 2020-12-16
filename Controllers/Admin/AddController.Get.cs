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
            if (request.RuleId > 0)
            {
                rule = await _ruleRepository.GetAsync(request.RuleId);
                contentHtmlClearList = ListUtils.GetStringList(rule.ContentHtmlClearCollection);
                contentHtmlClearTagList = ListUtils.GetStringList(rule.ContentHtmlClearTagCollection);
            }
            else
            {
                rule = new Rule
                {
                    SiteId = request.SiteId,
                    Charset = Charset.Utf8,
                    IsSaveImage = true,
                    ImageSource = ImageSource.None,
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

            var channel = await _channelRepository.GetAsync(rule.ChannelId);
            var channelIds = new List<int>();
            if (channel != null)
            {
                channelIds = ListUtils.GetIntList(channel.ParentsPath);
                channelIds.Add(rule.ChannelId);
                channelIds.Remove(site.Id);
            }

            return new GetResult
            {
                Rule = rule,
                Channels = channels,
                ChannelIds = channelIds,
                CharsetList = charsetList,
                ContentHtmlClearList = contentHtmlClearList,
                ContentHtmlClearTagList = contentHtmlClearTagList
            };
        }
    }
}
