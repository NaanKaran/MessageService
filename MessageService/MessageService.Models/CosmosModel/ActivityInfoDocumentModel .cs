using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.CosmosModel
{
    public class ActivityInfoDocumentModel : CosmosBaseModel

    {
        [JsonProperty("partitionkey")] public string PartitionKey => CosmosDocumentType.Activity.ToString();
        [JsonProperty("interactionid")] public string InteractionId { get; set; }
        [JsonProperty("journeyid")] public string JourneyId { get; set; }
        [JsonProperty("activityname")] public string ActivityName { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("createdon")] public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("type")] public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.Activity;
    }
}
