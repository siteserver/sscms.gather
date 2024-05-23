using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Enums;
using SSCMS.Gather.Core;
using SSCMS.Gather.Models;
using SSCMS.Models;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class TasksController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] SiteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsTasks))
            {
                return Unauthorized();
            }

            var cloudType = await _cloudManager.GetCloudTypeAsync();
            var taskIntervals = ListUtils.GetSelects<TaskInterval>();
            var allTasks = await _scheduledTaskRepository.GetAllAsync();
            var tasks = new List<ScheduledTask>();
            foreach (var task in allTasks)
            {
                if (StringUtils.EqualsIgnoreCase(task.TaskType, GatherManager.TaskType))
                {
                    var settings = TranslateUtils.JsonDeserialize<GatherTaskSettings>(task.Settings);
                    if (settings != null && settings.SiteId == request.SiteId)
                    {
                        task.Set("RuleIds", settings.RuleIds);
                        tasks.Add(task);
                    }
                }
            }

            var rules = new List<Select<int>>();
            foreach (var rule in await _ruleRepository.GetRulesAsync(request.SiteId))
            {
                 rules.Add(new Select<int>(rule.Id, rule.RuleName));
            }

            return new GetResult
            {
                CloudType = cloudType,
                TaskIntervals = taskIntervals,
                Tasks = tasks,
                Rules = rules,
            };
        }
    }
}
