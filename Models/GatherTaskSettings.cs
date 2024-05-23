using System.Collections.Generic;

namespace SSCMS.Gather.Models
{
    public class GatherTaskSettings
    {
        public int SiteId { get; set; }
        public List<int> RuleIds { get; set; }
    }
}
