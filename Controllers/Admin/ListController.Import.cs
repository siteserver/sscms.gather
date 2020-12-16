using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Gather.Core;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class ListController
    {
        [RequestSizeLimit(long.MaxValue)]
        [HttpPost, Route(RouteImport)]
        public async Task<ActionResult<BoolResult>> Import([FromQuery] SiteRequest request, [FromForm] IFormFile file)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, GatherManager.PermissionsList))
            {
                return Unauthorized();
            }

            if (file == null)
            {
                return this.Error("请选择有效的文件上传");
            }

            var fileName = Path.GetFileName(file.FileName);

            var extName = PathUtils.GetExtension(fileName);

            if (!StringUtils.EqualsIgnoreCase(".json", extName))
            {
                return this.Error("上传格式错误，请上传json文件!");
            }

            var filePath = _pathManager.GetTemporaryFilesPath(fileName);
            FileUtils.DeleteFileIfExists(filePath);
            await _pathManager.UploadAsync(file, filePath);

            await _gatherManager.ImportAsync(request.SiteId, filePath, false);

            FileUtils.DeleteFileIfExists(filePath);

            return Ok(new
            {
                Value = true
            });
        }
    }
}
