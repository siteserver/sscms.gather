using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Enums;
using SSCMS.Gather.Abstractions;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;

namespace SSCMS.Gather.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class TasksController : ControllerBase
    {
        private const string Route = "gather/tasks";
        private const string RouteDelete = "gather/tasks/actions/delete";
        private const string RouteEnable = "gather/tasks/actions/enable";

        private readonly IAuthManager _authManager;
        private readonly ICloudManager _cloudManager;
        private readonly IConfigRepository _configRepository;
        private readonly IScheduledTaskRepository _scheduledTaskRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IRuleRepository _ruleRepository;

        public TasksController(IAuthManager authManager, ICloudManager cloudManager, IConfigRepository configRepository, IScheduledTaskRepository scheduledTaskRepository, ISiteRepository siteRepository, IRuleRepository ruleRepository)
        {
            _authManager = authManager;
            _cloudManager = cloudManager;
            _configRepository = configRepository;
            _scheduledTaskRepository = scheduledTaskRepository;
            _siteRepository = siteRepository;
            _ruleRepository = ruleRepository;
        }

        public class GetResult
        {
            public CloudType CloudType { get; set; }
            public List<Select<string>> TaskIntervals { get; set; }
            public List<ScheduledTask> Tasks { get; set; }
            public List<Select<int>> Rules { get; set; }
        }

        public class SubmitRequest
        {
            public int SiteId { get; set; }
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public TaskInterval TaskInterval { get; set; }
            public int Every { get; set; }
            public List<int> Weeks { get; set; }
            public DateTime StartDate { get; set; }
            public bool IsNoticeSuccess { get; set; }
            public bool IsNoticeFailure { get; set; }
            public int NoticeFailureCount { get; set; }
            public bool IsNoticeMobile { get; set; }
            public string NoticeMobile { get; set; }
            public bool IsNoticeMail { get; set; }
            public string NoticeMail { get; set; }
            public bool IsDisabled { get; set; }
            public int Timeout { get; set; }
            public List<int> RuleIds { get; set; }
        }

        public class IdRequest : SiteRequest
        {
            public int Id { get; set; }
        }
    }
}
