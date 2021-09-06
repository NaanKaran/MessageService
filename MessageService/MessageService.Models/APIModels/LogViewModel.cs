using System;
using System.Runtime.Serialization;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.APIModels
{
    public class LogViewModel
    {
        [JsonProperty("sendid")] public string SendId { get; set; }
        [JsonProperty("journeyname")] public string JourneyName { get; set; }
        [IgnoreDataMember] public string Version { get; set; }
        [JsonProperty("version")] public string VersionName => $"Version {Version}";        
        [JsonProperty("activityname")] public string ActivityName { get; set; } 
        [JsonProperty("mobilenumber")] public string MobileNumber { get; set; }
        [JsonProperty("smscontent")] public string SMSContent { get; set; }
        [JsonProperty("senddate")] public DateTime SendDate { get; set; } 
        [JsonProperty("deliverydate")] public DateTime? DeliveryDate { get; set; } 
        [JsonProperty("sentstatus")] public SendStatus SentStatus { get; set; } 
        [JsonProperty("deliverystatus")] public DeliveryStatus? DeliveryStatus { get; set; }
        [JsonProperty("deliverydatestring")] public string DeliveryDateString => DeliveryDate.IsNotNull() ? DeliveryDate.Value.ToDateTimeString() : "";
        [JsonProperty("senddatestring")] public string SendDateString => SendDate.ToDateTimeString();
        [JsonProperty("quadrantinfo")] public string QuadrantInfo => SendDate.GetQuadrantMMSLogTableName(); // MMS Quadrant Info
        [JsonProperty("droperrorcode")] public string DropErrorCode { get; set; }
        [JsonProperty("errormessage")] public string ErrorMessage { get; set; }
        [JsonIgnore] public string InteractionId { get; set; }
        [JsonIgnore] public long AccountId { get; set; }
        [JsonIgnore] public string JourneyId { get; set; }
        //[JsonIgnore]
        [JsonProperty("activityid")]
        public string ActivityId { get; set; }
        [JsonIgnore] public string ActivityInteractionId { get; set; }
        [JsonIgnore] public string DeliveryStatusString => DeliveryStatus.IsNotNull() ? DeliveryStatus.ToString():"";
        [JsonIgnore] public string SentStatusString => SentStatus.ToString();

        //MMS
        [JsonProperty("mmstemplatename")] public string MMSTemplateName { get; set; }
        [JsonProperty("mmstemplateid")] public string MMSTemplateId { get; set; }
        [JsonProperty("dynamicparamsvalue")] public string DynamicParamsValue { get; set; }
    }

}
