using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.CosmosModel
{
    public class InteractionInfoDocumentModel : CosmosBaseModel

    {
        [JsonProperty("partitionkey")] public string PartitionKey => CosmosDocumentType.Interaction.ToString();
        [JsonProperty("journeyid")] public string JourneyId { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("publisheddate")] public DateTime? PublishedDate { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("createdon")] public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("type")] public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.Interaction;
    }
}
