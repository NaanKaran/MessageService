using System.Collections.Generic;
using Newtonsoft.Json;

namespace MessageService.Models.SubmailModel
{
    public class SubmailMmsTemplateModel : BaseSubmailModel
    {
        [JsonProperty("mms_title")] public string TemplateName { get; set; }
        [JsonProperty("mms_signature")] public string Signature { get; set; }
        [JsonProperty("mms_subject")] public string Title { get; set; }
       // [JsonProperty("mms_content")] public List<MmsContentModel> Content { get; set; }
        [JsonProperty("mms_content")] public string Content { get; set; }
        [JsonProperty("template_id")] public string TemplateId { get; set; }
    }
}
