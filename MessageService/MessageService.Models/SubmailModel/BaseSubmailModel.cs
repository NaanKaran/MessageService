using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class BaseSubmailModel
    {
        [JsonProperty("appid")] public string AppId { get; set; }
        [JsonProperty("signature")] public string AppKey { get; set; }
    }
}
