using System.Collections.Generic;

namespace SSCMS.Gather.Models
{
    public class Config
    {
        public bool IsEnabled { get; set; }

        public bool IsAllChannels { get; set; }

        public List<int> GatherChannels { get; set; }

        public bool IsAllAreas { get; set; }

        public List<int> GatherAreas { get; set; }

        public string GatherMethod { get; set; }

        public string RedirectUrl { get; set; }

        public string Warning { get; set; }

        public string Password { get; set; }
    }
}