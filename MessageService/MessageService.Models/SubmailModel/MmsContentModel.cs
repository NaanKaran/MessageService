using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class MmsContentModel
    {
        [JsonProperty("text")] public string TextMessage { get; set; }
    }

    public class MmsImageContentModel: MmsContentModel
    {
        [JsonProperty("image")] public Base64ContentModel Image { get; set; }
    }

    public class MmsAudioContentModel : MmsContentModel
    {
        [JsonProperty("audio")] public Base64ContentModel Audio { get; set; }
    }
    public class Base64ContentModel
    {
        [JsonProperty("data")] public string Base64String { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
    }

}
