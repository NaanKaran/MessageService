using Newtonsoft.Json;
using System;
using MessageService.Models.Enum;

namespace MessageService.Models.CosmosModel
{
    public abstract class CosmosBaseModel
    {
        [JsonProperty("id")] public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")] public abstract CosmosDocumentType Type { get; set; }
    }
}
