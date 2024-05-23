using System.Collections.Generic;
using System.Threading.Tasks;
using SSCMS.Gather.Core;
using SSCMS.Gather.Models;

namespace SSCMS.Gather.Abstractions
{
    public interface IGatherManager
    {
        ProgressCache GetCache(string guid);

        void Start(int adminId, int siteId, int ruleId, string guid);

        string Single(int adminId, int siteId, int ruleId, int channelId, List<Item> items);

        Task ExportAsync(Rule rule, string filePath);

        Task ImportAsync(int siteId, string filePath, bool overwrite);

        Task GatherChannelsAsync(int adminId, int siteId, int ruleId, string guid);
    }
}
