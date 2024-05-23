using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Gather.Abstractions;
using SSCMS.Dto;
using SSCMS.Gather.Core;
using SSCMS.Services;
using SSCMS.Gather.Models;
using SSCMS.Repositories;

namespace SSCMS.Gather.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class StatusController : ControllerBase
    {
        private const string Route = "gather/status";

        private readonly IAuthManager _authManager;
        private readonly IGatherManager _gatherManager;
        private readonly IRuleRepository _ruleRepository;
        private readonly IChannelRepository _channelRepository;

        public StatusController(IAuthManager authManager, IGatherManager gatherManager, IRuleRepository ruleRepository, IChannelRepository channelRepository)
        {
            _authManager = authManager;
            _gatherManager = gatherManager;
            _ruleRepository = ruleRepository;
            _channelRepository = channelRepository;
        }

        public class GetRequest : SiteRequest
        {
            public string RuleIds { get; set; }
        }

        public class GetResult
        {
            public List<Rule> Rules { get; set; }
        }

        public class SubmitRequest : SiteRequest
        {
            public string RuleIds { get; set; }
        }

        public class SubmitResult
        {
            public List<ProgressCache> Caches { get; set; }
        }
    }
}
