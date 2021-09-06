using MessageService.Models.Enum;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MessageService.Models.CosmosModel
{
    public class CategoryDocumentModel : CosmosBaseModel
    {
        [JsonProperty("partitionkey")]
        public string PartitionKey => CosmosDocumentType.TopicSubscriptionCategory.ToString();
        [JsonProperty("categoryname")]
        [Required]
        public string CategoryName { get; set; }
        [JsonProperty("type")] public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.TopicSubscriptionCategory;
    }
}
