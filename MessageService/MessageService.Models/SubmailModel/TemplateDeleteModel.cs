
using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class TemplateDeleteModel:BaseSubmailModel
    {
        [JsonProperty("template_id")] public string TemplateId { get; set; }
    }
}
