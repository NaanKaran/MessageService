using System;
using System.Collections.Generic;
using System.Linq;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.CosmosModel
{
    public class IncomingMessageDocumentModel : CosmosBaseModel

    {
        [JsonProperty("partitionkey")] public string PartitionKey => CosmosDocumentType.IncomingMessage.ToString();
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
        [JsonProperty("createdon")] public DateTime CreatedOn { get; set; }
        [JsonProperty("createdonstring")] public string CreatedOnString => CreatedOn.ToDateTimeString();
        [JsonProperty("mobilenumber")] public string MobileNumber { get; set; }
        [JsonProperty("journeyname")] public string JourneyName { get; set; }
        [JsonProperty("isoptout")] public bool IsOptOut => OptOutCharacters.Contains(Content?.Trim().ToUpper() ?? "");
        [JsonProperty("isupdatedintode")] public bool IsUpdatedIntoDE { get; set; }
        [JsonProperty("type")] public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.IncomingMessage;
        [JsonIgnore]private static IEnumerable<string> OptOutCharacters => new[] { "T", "TD", "N" };        
    }
}
