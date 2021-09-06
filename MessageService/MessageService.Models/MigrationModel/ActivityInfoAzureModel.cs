using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace MessageService.Models.MigrationModel
{
    public class ActivityInfoAzureModel : TableEntity
    {
        public ActivityInfoAzureModel()
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = CosmosDocumentType.Activity.ToString();
        }
        [JsonProperty("id")] public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("interactionid")] public string InteractionId { get; set; }
        [JsonProperty("journeyid")] public string JourneyId { get; set; }
        [JsonProperty("activityname")] public string ActivityName { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("createdon")] public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("type")] public int Type { get; set; } = (int)CosmosDocumentType.Activity;
    }
}
