using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Gather.Core;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class AddController
    {
        [HttpPost, Route(RouteActionsAttributes)]
        public async Task<ActionResult<AttributesResult>> Attributes([FromBody] AttributesRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsAdd))
            {
                return Unauthorized();
            }

            var attributes = new List<Option<string>>();
            var site = await _siteRepository.GetAsync(request.SiteId);
            var channel = await _channelRepository.GetAsync(request.ChannelId);
            var styles = await _tableStyleRepository.GetContentStylesAsync(site, channel);
            var selectedAttributes = new List<string>();

            if (request.RuleId > 0)
            {
                var rule = await _ruleRepository.GetAsync(request.RuleId);
                selectedAttributes = ListUtils.GetStringList(rule.ContentAttributes);
            }

            foreach (var style in styles)
            {
                if (StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.Title))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.Body))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.Id))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.LastModifiedDate))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.AdminId))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.UserId))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.SourceId))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.HitsByDay))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.HitsByWeek))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.HitsByMonth))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, nameof(SSCMS.Models.Content.LastHitsDate))
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, "CheckUserName")
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, "CheckDate")
                    || StringUtils.EqualsIgnoreCase(style.AttributeName, "CheckReasons")) continue;

                var listItem = new Option<string>
                {
                    Value = style.AttributeName,
                    Label = style.DisplayName
                };
                if (ListUtils.ContainsIgnoreCase(selectedAttributes, style.AttributeName))
                {
                    listItem.Selected = true;
                }
                attributes.Add(listItem);
            }

            var addDateOption = new Option<string>(nameof(SSCMS.Models.Content.AddDate), "添加日期");
            if (ListUtils.ContainsIgnoreCase(selectedAttributes, addDateOption.Value))
            {
                addDateOption.Selected = true;
            }
            attributes.Add(addDateOption);

            var hitsOption = new Option<string>(nameof(SSCMS.Models.Content.Hits), "点击量");
            if (ListUtils.ContainsIgnoreCase(selectedAttributes, hitsOption.Value))
            {
                hitsOption.Selected = true;
            }
            attributes.Add(hitsOption);

            var fileNameOption = new Option<string>("FileName", "原页面文件名");
            if (ListUtils.ContainsIgnoreCase(selectedAttributes, fileNameOption.Value))
            {
                fileNameOption.Selected = true;
            }
            attributes.Add(fileNameOption);

            return new AttributesResult
            {
                Attributes = attributes,
                Styles = styles
            };
        }
    }
}
