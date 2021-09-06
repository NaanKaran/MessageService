using System;
using Newtonsoft.Json;

namespace MessageService.Models.ExportModels
{
    public class IncomingMessagesExportModel : EmailExportModel
    {
        [JsonProperty("mobilenumber")]
        public string MobileNumber { get; set; }
        [JsonProperty("journeyname")]
        public string JourneyName { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }      
        public DateTime ReceivedDate { get; set; }
    }
}
