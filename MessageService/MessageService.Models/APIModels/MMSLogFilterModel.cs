using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class LogFilterModel
    {
        [JsonProperty("journeyid")] public string JourneyId { get; set; }
        [JsonProperty("quadranttablename")] public string QuadrantTableName { get; set; }
        [JsonProperty("interactionid")] public string InteractionId { get; set; }
        [JsonProperty("activityid")] public string ActivityId { get; set; }
        [JsonProperty("mobilenumber")] public string MobileNumber { get; set; }
        [JsonProperty("sendstatus")] public SendStatus? SendStatus { get; set; }
        [JsonProperty("deliverystatus")] public DeliveryStatus? DeliveryStatus { get; set; }
        [JsonProperty("senddatefrom")] public DateTime? SendDateFrom { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("emailids")] public string[] EmailIds { get; set; }
        [JsonProperty("pageno")] public int PageNo { get; set; }
        [JsonProperty("itemsperpage")] public int ItemsPerPage { get; set; }

        [JsonProperty("senddateto")]
        public DateTime? SendDateTo { get; set; }
        [JsonProperty("deliverydatefrom")] public DateTime? DeliveryDateFrom { get; set; }

        [JsonProperty("deliverydateto")]
        public DateTime? DeliveryDateTo { get; set; }

    }
}
