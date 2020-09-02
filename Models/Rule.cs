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

        [DataColumn(Text = true)] public string CookieString { get; set; }

        [DataColumn] public bool GatherUrlIsCollection { get; set; }

        [DataColumn(Text = true)] public string GatherUrlCollection { get; set; }

        [DataColumn] public bool GatherUrlIsSerialize { get; set; }

        [DataColumn(Text = true)] public string GatherUrlSerialize { get; set; }

        [DataColumn] public int SerializeFrom { get; set; }

        [DataColumn] public int SerializeTo { get; set; }

        [DataColumn] public int SerializeInterval { get; set; }

        [DataColumn] public bool SerializeIsOrderByDesc { get; set; }

        [DataColumn] public bool SerializeIsAddZero { get; set; }

        [DataColumn] public int ChannelId { get; set; }

        [DataColumn] public Charset Charset { get; set; }

        [DataColumn(Text = true)] public string UrlInclude { get; set; }

        [DataColumn(Text = true)] public string TitleInclude { get; set; }

        [DataColumn(Text = true)] public string ContentExclude { get; set; }

        [DataColumn(Text = true)] public string ContentHtmlClearCollection { get; set; }

        [DataColumn(Text = true)] public string ContentHtmlClearTagCollection { get; set; }

        [DataColumn] public DateTime? LastGatherDate { get; set; }

        [DataColumn(Text = true)] public string ListAreaStart { get; set; }

        [DataColumn(Text = true)] public string ListAreaEnd { get; set; }

        [DataColumn(Text = true)] public string ContentChannelStart { get; set; }

        [DataColumn(Text = true)] public string ContentChannelEnd { get; set; }

        [DataColumn(Text = true)] public string ContentTitleStart { get; set; }

        [DataColumn(Text = true)] public string ContentTitleEnd { get; set; }

        [DataColumn(Text = true)] public string ContentContentStart { get; set; }

        [DataColumn(Text = true)] public string ContentContentEnd { get; set; }

        [DataColumn(Text = true)] public string ContentNextPageStart { get; set; }

        [DataColumn(Text = true)] public string ContentNextPageEnd { get; set; }

        [DataColumn(Text = true)] public string ContentAttributes { get; set; }

        public int GatherNum { get; set; }

        public bool IsSaveImage { get; set; }

        public bool IsSetFirstImageAsImageUrl { get; set; }

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

        public string ContentReplaceFrom { get; set; }

        public string ContentReplaceTo { get; set; }
    }
}
