using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Utils;

namespace SSCMS.Gather.Controllers.Admin
{
    public partial class ListController
    {
        [HttpGet, Route(RouteExport)]
        public async Task<FileResult> Export([FromQuery] ExportRequest request)
        {
            var rule = await _ruleRepository.GetAsync(request.RuleId);

            var filePath = _pathManager.GetTemporaryFilesPath($"{rule.RuleName}.json");
            FileUtils.DeleteFileIfExists(filePath);

            await _gatherManager.ExportAsync(rule, filePath);

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, MediaTypeNames.Application.Octet, Path.GetFileName(filePath));
        }
    }
}
