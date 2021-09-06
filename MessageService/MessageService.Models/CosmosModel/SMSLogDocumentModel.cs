using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.CosmosModel
{
    public class SMSLogDocumentModel : CosmosBaseModel
    {
        [JsonProperty("partitionkey")] public string PartitionKey { get; set; }
        [JsonProperty("mobilenumber")] public string MobileNumber { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("sentstatus")] public SendStatus SentStatus { get; set; }
        [JsonProperty("senddate")] public DateTime SendDate { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("deliverystatus")] public DeliveryStatus? DeliveryStatus { get; set; }
        [JsonProperty("deliverydate")] public DateTime? DeliveryDate { get; set; }
        [JsonProperty("droperrorcode")] public string DropErrorCode { get; set; }
        [JsonProperty("errormessage")] public string ErrorMessage { get; set; }
        [JsonProperty("interactionid")] public string InteractionId { get; set; }
        [JsonProperty("journeyid")] public string JourneyId { get; set; }        
        [JsonProperty("activityid")] public string ActivityId { get; set; }
        [JsonProperty("credit")] public int Credit { get; set; }
        [JsonProperty("contactkey")] public string ContactKey { get; set; }
        [JsonProperty("smscontent")] public string SMSContent { get; set; }
        [JsonProperty("personalizationdata")] public string PersonalizationData { get; set; }
        [JsonProperty("isupdatedtode")] public bool IsUpdatedToDE { get; set; }
        [JsonProperty("type")] public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.Log;
    }
}

