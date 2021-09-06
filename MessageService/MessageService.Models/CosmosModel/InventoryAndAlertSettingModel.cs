using MessageService.Models.Enum;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MessageService.Models.CosmosModel
{
    public class InventoryAndAlertSettingDocumentModel : CosmosBaseModel
    {
        [JsonProperty("partitionkey")] public string PartitionKey => CosmosDocumentType.InventoryAndAlert.ToString();
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("alertthreshold")] public int AlertThreshold { get; set; }
        [JsonProperty("updatedby")] public string UpdatedBy { get; set; }
        [JsonProperty("isconfigured")] public bool IsConfigured { get; set; }
        [JsonProperty("type")] public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.InventoryAndAlert;        
        [JsonProperty("notificationusers")] public List<EmailNotificationUserDocumentModel> NotificationUsers { get; set; } = new List<EmailNotificationUserDocumentModel>();

    }
}
