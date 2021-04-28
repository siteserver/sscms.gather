using System;
using Datory;
using Datory.Annotations;

namespace SSCMS.Gather.Models
{
    [DataTable("sscms_gather_rule")]
    public class Rule : Entity
    {
        [DataColumn] public string RuleName { get; set; }
        [DataColumn] public int SiteId { get; set; }
        [DataColumn] public DateTime? LastGatherDate { get; set; }
        public string CookieString { get; set; }
        public bool GatherUrlIsCollection { get; set; }
        public string GatherUrlCollection { get; set; }
        public bool GatherUrlIsSerialize { get; set; }
        public string GatherUrlSerialize { get; set; }
        public int SerializeFrom { get; set; }
        public int SerializeTo { get; set; }
        public int SerializeInterval { get; set; }
        public bool SerializeIsOrderByDesc { get; set; }
        public bool SerializeIsAddZero { get; set; }
        public int ChannelId { get; set; }
        public Charset Charset { get; set; }
        public string ContentUrlStart { get; set; }
        public string ContentUrlEnd { get; set; }
        public string ImageUrlStart { get; set; }
        public string ImageUrlEnd { get; set; }
        public string TitleInclude { get; set; }
        public string ContentExclude { get; set; }
        public string ContentHtmlClearCollection { get; set; }
        public string ContentHtmlClearTagCollection { get; set; }
        public string ListAreaStart { get; set; }
        public string ListAreaEnd { get; set; }
        public string ContentChannelStart { get; set; }
        public string ContentChannelEnd { get; set; }
        public bool ContentTitleByList { get; set; }
        public string ContentTitleStart { get; set; }
        public string ContentTitleEnd { get; set; }
        public string ContentContentStart { get; set; }
        public string ContentContentEnd { get; set; }
        public string ContentNextPageStart { get; set; }
        public string ContentNextPageEnd { get; set; }
        public string ContentAttributes { get; set; }
        public int GatherNum { get; set; }
        public bool IsSaveImage { get; set; }
        public ImageSource ImageSource { get; set; }
        public bool IsSaveFiles { get; set; }
        public bool IsEmptyContentAllowed { get; set; }
        public bool IsSameTitleAllowed { get; set; }
        public bool IsChecked { get; set; }
        public bool IsAutoCreate { get; set; }
        public bool IsOrderByDesc { get; set; }
        public string ContentContentStart2 { get; set; }
        public string ContentContentEnd2 { get; set; }
        public string ContentContentStart3 { get; set; }
        public string ContentContentEnd3 { get; set; }
        public string FileNameAttributeName { get; set; }
    }
}
