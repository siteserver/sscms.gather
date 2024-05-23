using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Gather.Abstractions;
using SSCMS.Dto;
using SSCMS.Gather.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class ListController : ControllerBase
    {
        private const string Route = "gather/list";
        private const string RouteExport = "gather/list/actions/export";
        private const string RouteImport = "gather/list/actions/import";
        private const string RouteDelete = "gather/list/actions/delete";

        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly IGatherManager _gatherManager;
        private readonly IRuleRepository _ruleRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly ITableStyleRepository _tableStyleRepository;

        public ListController(IAuthManager authManager, IPathManager pathManager, IGatherManager gatherManager, IRuleRepository ruleRepository, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository, ITableStyleRepository tableStyleRepository)
        {
            _authManager = authManager;
            _pathManager = pathManager;
            _gatherManager = gatherManager;
            _ruleRepository = ruleRepository;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _tableStyleRepository = tableStyleRepository;
        }

        public class GetResult
        {
            public List<Rule> Rules { get; set; }
        }

        public class DeleteRequest : SiteRequest
        {
            public List<int> RuleIds { get; set; }
        }

        public class DeleteResult
        {
            public List<Rule> Rules { get; set; }
        }

        public class ExportRequest : SiteRequest
        {
            public int RuleId { get; set; }
        }
    }
}
