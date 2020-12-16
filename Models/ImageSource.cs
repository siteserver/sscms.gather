using System.Runtime.Serialization;
using Datory.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SSCMS.Gather.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageSource
    {
        [EnumMember(Value = "none")]
        [DataEnum(Value = "none")]
        None,
        [EnumMember(Value = "content")]
        [DataEnum(Value = "content")]
        Content,
        [EnumMember(Value = "list")]
        [DataEnum(Value = "list")]
        List
    }
}
