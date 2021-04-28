using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Gather.Core;
using SSCMS.Gather.Models;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class AddController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsAdd))
            {
                return Unauthorized();
            }

            var rule = new Rule
            {
                SiteId = request.SiteId
            };
            if (request.Id > 0)
            {
                rule = await _ruleRepository.GetAsync(request.Id);
            }

            rule.RuleName = request.RuleName;
            rule.ChannelId = request.ChannelId;
            rule.Charset = request.Charset;
            rule.GatherNum = request.GatherNum;
            rule.IsSaveImage = request.IsSaveImage;
            rule.IsSaveFiles = request.IsSaveFiles;
            rule.ImageSource = request.ImageSource;
            rule.IsEmptyContentAllowed = request.IsEmptyContentAllowed;
            rule.IsSameTitleAllowed = request.IsSameTitleAllowed;
            rule.IsChecked = request.IsChecked;
            rule.IsOrderByDesc = request.IsOrderByDesc;
            rule.GatherUrlIsCollection = request.GatherUrlIsCollection;
            rule.GatherUrlIsSerialize = request.GatherUrlIsSerialize;
            rule.GatherUrlCollection = request.GatherUrlCollection;
            rule.GatherUrlSerialize = request.GatherUrlSerialize;
            rule.SerializeFrom = request.SerializeFrom;
            rule.SerializeTo = request.SerializeTo;
            rule.SerializeInterval = request.SerializeInterval;
            rule.SerializeIsOrderByDesc = request.SerializeIsOrderByDesc;
            rule.SerializeIsAddZero = request.SerializeIsAddZero;
            rule.ContentUrlStart = request.ContentUrlStart;
            rule.ContentUrlEnd = request.ContentUrlEnd;
            rule.ImageUrlStart = request.ImageUrlStart;
            rule.ImageUrlEnd = request.ImageUrlEnd;
            rule.ContentTitleByList = request.ContentTitleByList;
            rule.ContentTitleStart = request.ContentTitleStart;
            rule.ContentTitleEnd = request.ContentTitleEnd;
            rule.ContentContentStart = request.ContentContentStart;
            rule.ContentContentEnd = request.ContentContentEnd;
            rule.ContentContentStart2 = request.ContentContentStart2;
            rule.ContentContentEnd2 = request.ContentContentEnd2;
            rule.ContentContentStart3 = request.ContentContentStart3;
            rule.ContentContentEnd3 = request.ContentContentEnd3;
            rule.ContentNextPageStart = request.ContentNextPageStart;
            rule.ContentNextPageEnd = request.ContentNextPageEnd;
            rule.TitleInclude = request.TitleInclude;
            rule.ListAreaStart = request.ListAreaStart;
            rule.ListAreaEnd = request.ListAreaEnd;
            rule.CookieString = request.CookieString;
            rule.ContentExclude = request.ContentExclude;
            rule.ContentHtmlClearCollection = ListUtils.ToString(request.ContentHtmlClearList);
            rule.ContentHtmlClearTagCollection = ListUtils.ToString(request.ContentHtmlClearTagList);
            rule.FileNameAttributeName = request.FileNameAttributeName;

            rule.ContentAttributes = ListUtils.ToString(request.ContentAttributeList);
            if (request.ContentAttributeList != null)
            {
                foreach (var attribute in request.ContentAttributeList)
                {
                    rule.Set($"{attribute}ByList", request.Get<bool>($"{attribute}ByList"));
                    rule.Set($"{attribute}Start", request.Get<string>($"{attribute}Start"));
                    rule.Set($"{attribute}End", request.Get<string>($"{attribute}End"));
                    rule.Set($"{attribute}Default", request.Get<string>($"{attribute}Default"));
                }
            }

            if (rule.Id > 0)
            {
                await _ruleRepository.UpdateAsync(rule);
            }
            else
            {
                if (await _ruleRepository.IsExistsAsync(request.SiteId, rule.RuleName))
                {
                    return BadRequest("保存失败，已存在相同名称的采集规则！");
                }

                await _ruleRepository.InsertAsync(rule);
            }

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
