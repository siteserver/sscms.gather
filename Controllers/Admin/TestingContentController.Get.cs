using System.Collections.Generic;
using System.Linq;
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
            var items = GatherUtils.GetItems(request.ListUrl, rule);
            var item = items.FirstOrDefault(x => StringUtils.EqualsIgnoreCase(x.Url, request.ContentUrl));

            var attributes = GatherUtils.GetContentNameValueCollection(rule, item);

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
