using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.ExportModels
{
    public class JourneyExportModel : EmailExportModel
    {
        [JsonProperty("journeyid")]
        public string JourneyId { get; set; }
        [JsonProperty("journeyids")]
        public string[] JourneyIds { get; set; }
        [JsonProperty("emailexporttype")]
        public SMSEmailType EmailExportType { get; set; }
    }
}
