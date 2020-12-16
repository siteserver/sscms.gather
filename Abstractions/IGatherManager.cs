using System.Collections.Generic;
using System.Threading.Tasks;
using SSCMS.Gather.Core;
using SSCMS.Gather.Models;

namespace SSCMS.Gather.Abstractions
{
    public interface IGatherManager
    {
        ProgressCache InitCache(string guid, string message);

        ProgressCache GetCache(string guid);

        void Start(int adminId, int siteId, int ruleId, string guid);

        string Single(int adminId, int siteId, int ruleId, int channelId, List<string> contentUrls, List<string> imageUrls);

        Task GatherContentsAsync(int adminId, int siteId, int ruleId, int channelId,
            string guid, List<string> contentUrls, List<string> imageUrls);

        Task ExportAsync(Rule rule, string filePath);

        Task ImportAsync(int siteId, string filePath, bool overwrite);
    }
}
