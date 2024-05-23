using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Gather.Core;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class TasksController
    {
        [HttpPost, Route(RouteDelete)]
        public async Task<ActionResult<BoolResult>> Delete([FromBody] IdRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsTasks))
            {
                return Unauthorized();
            }

            await _scheduledTaskRepository.DeleteAsync(request.Id);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
