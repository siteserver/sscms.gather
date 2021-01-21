using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Gather.Abstractions;
using SSCMS.Dto;
using SSCMS.Gather.Models;
using SSCMS.Services;

namespace SSCMS.Gather.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class TestingController : ControllerBase
    {
        private const string Route = "gather/testing";

        private readonly IAuthManager _authManager;
        private readonly IRuleRepository _ruleRepository;

        public TestingController(IAuthManager authManager, IRuleRepository ruleRepository)
        {
            _authManager = authManager;
            _ruleRepository = ruleRepository;
        }

        public class GetRequest : SiteRequest
        {
            public int RuleId { get; set; }
        }

        public class GetResult
        {
            public Rule Rule { get; set; }
            public List<string> GatherUrls { get; set; }
        }

        public class SubmitRequest : SiteRequest
        {
            public int RuleId { get; set; }
            public string GatherUrl { get; set; }
        }

        public class SubmitResult
        {
            public List<Item> Items { get; set; }
        }
    }
}
