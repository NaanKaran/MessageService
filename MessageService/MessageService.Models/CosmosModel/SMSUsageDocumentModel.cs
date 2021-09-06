using MessageService.Models.Enum;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MessageService.Models.CosmosModel
{
    public class SMSUsageDocumentModel : CosmosBaseModel
    {
        [JsonProperty("partitionkey")]
        public string PartitionKey => CosmosDocumentType.Usage.ToString();
        [JsonProperty("id")] public new string Id => AccountId + MonthString + Year;

        [JsonProperty("month")]
        public Months Month { get; set; }
        [JsonProperty("monthstring")]
        public string MonthString => Month.ToString();
        [JsonProperty("year")]
        public int Year { get; set; }
        [JsonProperty("rechargecount")]
        public int RechargeCount { get; set; }
        [JsonProperty("usedcount")]
        public int UsedCount { get; set; }
        [JsonProperty("balance")]
        public string Balance { get; set; }
        [JsonProperty("accountid")]
        public long AccountId { get; set; }        
        [JsonProperty("type")]
        public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.Usage;
    }
}
