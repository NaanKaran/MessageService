using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace MessageService.Models.MigrationModel
{
    public class InteractionInfoAzureModel : TableEntity
    {
        public InteractionInfoAzureModel()
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = CosmosDocumentType.Interaction.ToString();
        }
        [JsonProperty("id")] public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("journeyid")] public string JourneyId { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("publisheddate")] public DateTime? PublishedDate { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("createdon")] public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("type")] public int Type { get; set; } = (int)CosmosDocumentType.Interaction;
    }
}
