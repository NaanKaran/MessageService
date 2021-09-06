using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class SubmailMMSPostModel
    {
       [JsonProperty("appid")] public string AppId { get; set; }
       [JsonProperty("to")] public string To { get; set; }
       [JsonProperty("project")] public string Project { get; set; }
       [JsonProperty("signature")] public string Signature { get; set; }
    }
}
