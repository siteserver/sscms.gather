using System.Collections.Generic;
using System.Threading.Tasks;
using SSCMS.Gather.Core;
using Config = SSCMS.Gather.Models.Config;

namespace SSCMS.Gather.Abstractions
{
    public interface IGatherManager
    {
        Task<Config> GetConfigAsync(int siteId);

        Task<bool> SetConfigAsync(int siteId, Config config);

        ProgressCache InitCache(string guid, string message);

        ProgressCache GetCache(string guid);

        void Start(int adminId, int siteId, int ruleId, string guid, bool isCli);

        string Single(int adminId, int siteId, int ruleId, int channelId, List<string> urls);

        Task GatherContentsAsync(int adminId, int siteId, int ruleId, int channelId,
            string guid, List<string> urls);
    }
}
