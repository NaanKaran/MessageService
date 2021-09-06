using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class TemplateUpdateModel
    {
        [JsonProperty("templateid")]
        public string TemplateId { get; set; }

        [JsonProperty("comments")]
        public string Comments { get; set; }

        [JsonProperty("status")]
        public TemplateStatus Status { get; set; }
        [JsonProperty("updatedon")]
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("updatedby")]
        public string UpdatedBy { get; set; }
    }
}
