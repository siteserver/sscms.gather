using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Gather.Abstractions;
using SSCMS.Dto;
using SSCMS.Gather.Core;
using SSCMS.Gather.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class SingleController : ControllerBase
    {
        private const string Route = "gather/single";
        private const string RouteActionsGather = "gather/single/actions/gather";
        private const string RouteActionsStatus = "gather/single/actions/status";

        private readonly IAuthManager _authManager;
        private readonly IGatherManager _gatherManager;
        private readonly IRuleRepository _ruleRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;

        public SingleController(IAuthManager authManager, IGatherManager gatherManager, IRuleRepository ruleRepository, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository)
        {
            _authManager = authManager;
            _gatherManager = gatherManager;
            _ruleRepository = ruleRepository;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
        }

        public class GetRequest : SiteRequest
        {
            public int RuleId { get; set; }
        }

        public class GetResult
        {
            public Rule Rule { get; set; }
            public List<Cascade<int>> Channels { get; set; }
        }

        public class SubmitRequest
        {
            public int SiteId { get; set; }
            public int RuleId { get; set; }
            public int ChannelId { get; set; }
            public bool IsChecked { get; set; }
            public bool GatherUrlIsCollection { get; set; }
            public bool GatherUrlIsSerialize { get; set; }
            public string GatherUrlCollection { get; set; }
            public string GatherUrlSerialize { get; set; }
            public int SerializeFrom { get; set; }
            public int SerializeTo { get; set; }
            public int SerializeInterval { get; set; }
            public bool SerializeIsOrderByDesc { get; set; }
            public bool SerializeIsAddZero { get; set; }
        }

        public class GatherRequest : SiteRequest
        {
            public int RuleId { get; set; }
            public string Guid { get; set; }
        }

        public class StatusRequest : SiteRequest
        {
            public string Guid { get; set; }
        }

        public class StatusResult
        {
            public ProgressCache Cache { get; set; }
        }
    }
}
