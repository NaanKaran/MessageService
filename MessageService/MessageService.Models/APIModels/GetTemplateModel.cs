using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class GetTemplateModel
    {
        [Required]
        [JsonProperty("accountid")]
        public long AccountId { get; set; }
        [JsonProperty("templatename")]
        public string TemplateName { get; set; }
        [JsonProperty("status")]
        public List<TemplateStatus> Status { get; set; } = new List<TemplateStatus>();
        [JsonProperty("pageno")]
        public int PageNo { get; set; }
        [JsonProperty("itemsperpage")]
        public int ItemsPerPage { get; set; }
    }
}
