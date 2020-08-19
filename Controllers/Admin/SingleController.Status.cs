using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Gather.Core;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class SingleController
    {
        [HttpPost, Route(RouteActionsStatus)]
        public async Task<ActionResult<StatusResult>> Status([FromBody] StatusRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            var cache = _gatherManager.GetCache(request.Guid);

            return new StatusResult
            {
                Cache = cache
            };
        }
    }
}
