using System.Runtime.Serialization;
using Datory.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SSCMS.Gather.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Charset
    {
        [EnumMember(Value = "utf-8")]
        [DataEnum(DisplayName = "Unicode (UTF-8)", Value = "utf-8")]
        Utf8,
        [EnumMember(Value = "gb2312")]
        [DataEnum(DisplayName = "简体中文 (GB2312)", Value = "gb2312")]
        Gb2312,
        [EnumMember(Value = "big5")]
        [DataEnum(DisplayName = "繁体中文 (Big5)", Value = "big5")]
        Big5
    }
}
