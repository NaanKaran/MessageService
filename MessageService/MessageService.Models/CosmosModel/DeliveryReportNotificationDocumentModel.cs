using System;
using System.Collections.Generic;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.CosmosModel
{
    public class DeliveryReportNotificationDocumentModel : CosmosBaseModel
    {
        [JsonProperty("partitionkey")] public string PartitionKey => CosmosDocumentType.DeliveryReportNotification.ToString();
        [JsonProperty("runby")] public RunBy RunBy { get; set; }
        [JsonProperty("dayon")] public uint DayOn { get; set; }
        [JsonProperty("runon")] public int RunOn { get; set; }
        [JsonProperty("deliverypercentage")] public int DeliveryPercentage { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("createdon")] public DateTime CreatedOn { get; set; } = DateTime.Now.ToChinaTime();

        [JsonProperty("type")]
        public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.DeliveryReportNotification;
        [JsonProperty("notify")] public string Notify { get; set; } = "Not Yet Configured.";
        [JsonProperty("isconfigured")] public bool IsConfigured { get; set; }
        [JsonProperty("notificationusers")]
        public List<EmailNotificationUserDocumentModel> NotificationUsers { get; set; } =
            new List<EmailNotificationUserDocumentModel>();        
    }
}
