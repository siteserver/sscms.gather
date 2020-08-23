using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Gather.Abstractions;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class LayerCopyController : ControllerBase
    {
        private const string Route = "gather/layer/copy";

        private readonly IAuthManager _authManager;
        private readonly IRuleRepository _ruleRepository;

        public LayerCopyController(IAuthManager authManager, IRuleRepository ruleRepository)
        {
            _authManager = authManager;
            _ruleRepository = ruleRepository;
        }
        public class SubmitRequest
        {
            public int SiteId { get; set; }
            public int RuleId { get; set; }
            public string RuleName { get; set; }
        }
    }
}
