using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Gather.Core;
using SSCMS.Gather.Models;
using SSCMS.Models;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class TasksController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsTasks))
            {
                return Unauthorized();
            }

            var task = new ScheduledTask();
            if (request.Id > 0)
            {
                task = await _scheduledTaskRepository.GetAsync(request.Id);
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.AdminId = _authManager.AdminId;
            task.TaskType = GatherManager.TaskType;
            task.TaskInterval = request.TaskInterval;
            task.Every = request.Every;
            task.Weeks = request.Weeks;
            task.StartDate = request.StartDate;
            task.IsNoticeSuccess = request.IsNoticeSuccess;
            task.IsNoticeFailure = request.IsNoticeFailure;
            task.NoticeFailureCount = request.NoticeFailureCount;
            task.IsNoticeMobile = request.IsNoticeMobile;
            task.NoticeMobile = request.NoticeMobile;
            task.IsNoticeMail = request.IsNoticeMail;
            task.NoticeMail = request.NoticeMail;
            task.IsDisabled = request.IsDisabled;
            task.Timeout = request.Timeout;

            var settings = new GatherTaskSettings
            {
                SiteId = request.SiteId,
                RuleIds = request.RuleIds,
            };
            task.Settings = TranslateUtils.JsonSerialize(settings);

            // if (task.TaskType == TaskType.Create)
            // {
            //     task.CreateSiteIds = request.CreateSiteIds;
            //     task.CreateType = request.CreateType;
            // }
            // else if (task.TaskType == TaskType.Ping)
            // {
            //     task.PingHost = request.PingHost;
            // }

            if (request.Id > 0)
            {
                await _scheduledTaskRepository.UpdateAsync(task);
            }
            else
            {
                await _scheduledTaskRepository.InsertAsync(task);
            }

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
