using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class MmsTemplateResponseModel
    {
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("template_id")] public string TemplateId { get; set; }
        [JsonProperty("code")] public string ErrorCode { get; set; }
        [JsonProperty("msg")] public string ErrorMessage { get; set; }
    }
}
