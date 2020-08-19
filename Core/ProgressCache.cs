using System.Collections.Generic;

namespace SSCMS.Gather.Core
{
    public class ProgressCache
    {
        public string Status { get; set; }
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<string> FailureMessages { get; set; }
    }
}