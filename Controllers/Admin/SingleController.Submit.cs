using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Gather.Core;
using SSCMS.Gather.Models;
using SSCMS.Models;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class SingleController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<StringResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var rule = await _ruleRepository.GetAsync(request.RuleId);

            rule.ChannelId = request.ChannelId;
            rule.IsChecked = request.IsChecked;

            await _ruleRepository.UpdateAsync(rule);

            var urls = ListUtils.GetStringList(request.Urls, '\n');
            var items = urls.Select(x => new Item
            {
                Url = x,
                Content = new Content()
            }).ToList();

            var guid = _gatherManager.Single(_authManager.AdminId, request.SiteId, request.RuleId, request.ChannelId, items);

            return new StringResult
            {
                Value = guid
            };
        }
    }
}
