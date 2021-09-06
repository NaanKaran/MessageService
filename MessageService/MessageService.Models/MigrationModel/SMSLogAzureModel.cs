using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace MessageService.Models.MigrationModel
{
    public class SMSLogAzureModel : TableEntity
    {
        public SMSLogAzureModel()
        {
            RowKey = Guid.NewGuid().ToString();
        }
        [JsonProperty("id")] public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("mobilenumber")] public string MobileNumber { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("sentstatus")] public int SentStatus { get; set; }
        [JsonProperty("senddate")] public DateTime SendDate { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("deliverystatus")] public int? DeliveryStatus { get; set; }
        [JsonProperty("deliverydate")] public DateTime? DeliveryDate { get; set; }
        [JsonProperty("droperrorcode")] public string DropErrorCode { get; set; }
        [JsonProperty("errormessage")] public string ErrorMessage { get; set; }
        [JsonProperty("interactionid")] public string InteractionId { get; set; }
        [JsonProperty("journeyid")] public string JourneyId { get; set; }
        [JsonProperty("activityid")] public string ActivityId { get; set; }
        [JsonProperty("smscontent")] public string SMSContent { get; set; }
        [JsonProperty("personalizationdata")] public string PersonalizationData { get; set; }
        [JsonProperty("isupdatedtode")] public bool IsUpdatedToDE { get; set; }
        [JsonProperty("contactkey")] public string ContactKey { get; set; }
        [JsonProperty("type")] public int Type { get; set; } =(int) CosmosDocumentType.Log;

    }
}
