using System;
using MessageService.InfraStructure.Helpers;
using Newtonsoft.Json;

namespace MessageService.Models.ExportModels
{
    public class EmailExportModel
    {
        [JsonProperty("emailids")]
        public string[] EmailIds { get; set; }
        [JsonProperty("accountid")]
        public long AccountId { get; set; }
        [JsonProperty("fromdate")]
        public DateTime FromDate { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("todate")]
        public DateTime ToDate { get; set; } = DateTime.UtcNow.ToChinaTime();

    }
}
