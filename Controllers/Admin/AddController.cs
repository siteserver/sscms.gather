using System.Collections.Generic;
using Datory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Gather.Abstractions;
using SSCMS.Dto;
using SSCMS.Gather.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Models;

namespace SSCMS.Gather.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class AddController : ControllerBase
    {
        private const string Route = "gather/add";
        private const string RouteActionsAttributes = "gather/add/actions/attributes";

        private readonly IAuthManager _authManager;
        private readonly IRuleRepository _ruleRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly ITableStyleRepository _tableStyleRepository;

        public AddController(IAuthManager authManager, IRuleRepository ruleRepository, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository, ITableStyleRepository tableStyleRepository)
        {
            _authManager = authManager;
            _ruleRepository = ruleRepository;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _tableStyleRepository = tableStyleRepository;
        }

        public class GetRequest : SiteRequest
        {
            public int RuleId { get; set; }
        }

        public class GetResult
        {
            public Rule Rule { get; set; }
            public List<Cascade<int>> Channels { get; set; }
            public List<int> ChannelIds { get; set; }
            public IEnumerable<Select<string>> CharsetList { get; set; }
            public List<string> ContentHtmlClearList { get; set; }
            public List<string> ContentHtmlClearTagList { get; set; }
        }

        public class AttributesRequest : SiteRequest
        {
            public int RuleId { get; set; }
            public int ChannelId { get; set; }
        }

        public class AttributesResult
        {
            public List<Option<string>> Attributes { get; set; }
            public List<TableStyle> Styles { get; set; }
        }

        public class SubmitRequest : Entity
        {
            public int SiteId { get; set; }
            public string RuleName { get; set; }
            public int ChannelId { get; set; }
            public Charset Charset { get; set; }
            public int GatherNum { get; set; }
            public bool IsSaveImage { get; set; }
            public ImageSource ImageSource { get; set; }
            public bool IsSaveFiles { get; set; }
            public bool IsEmptyContentAllowed { get; set; }
            public bool IsSameTitleAllowed { get; set; }
            public bool IsChecked { get; set; }
            public bool IsOrderByDesc { get; set; }
            public bool GatherUrlIsCollection { get; set; }
            public bool GatherUrlIsSerialize { get; set; }
            public string GatherUrlCollection { get; set; }
            public string GatherUrlSerialize { get; set; }
            public int SerializeFrom { get; set; }
            public int SerializeTo { get; set; }
            public int SerializeInterval { get; set; }
            public bool SerializeIsOrderByDesc { get; set; }
            public bool SerializeIsAddZero { get; set; }
            public string ContentUrlStart { get; set; }
            public string ContentUrlEnd { get; set; }
            public string ImageUrlStart { get; set; }
            public string ImageUrlEnd { get; set; }
            public bool ContentTitleByList { get; set; }
            public string ContentTitleStart { get; set; }
            public string ContentTitleEnd { get; set; }
            public string ContentContentStart { get; set; }
            public string ContentContentEnd { get; set; }
            public string ContentContentStart2 { get; set; }
            public string ContentContentEnd2 { get; set; }
            public string ContentContentStart3 { get; set; }
            public string ContentContentEnd3 { get; set; }
            public string ContentNextPageStart { get; set; }
            public string ContentNextPageEnd { get; set; }
            public string TitleInclude { get; set; }
            public string ListAreaStart { get; set; }
            public string ListAreaEnd { get; set; }
            public string CookieString { get; set; }
            public string ContentExclude { get; set; }
            public List<string> ContentHtmlClearList { get; set; }
            public List<string> ContentHtmlClearTagList { get; set; }
            public List<string> ContentAttributeList { get; set; }
            public Dictionary<string, string> ContentAttributesDict { get; set; }
            public string FileNameAttributeName { get; set; }
        }
    }
}
