using System;
using MessageService.InfraStructure.Helpers;
using Newtonsoft.Json;

namespace MessageService.Models.MMSModels
{
    public class MMSLibraryModel
    {
        [JsonProperty("id")] public Guid Id { get; set; } = Guid.NewGuid();
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("extension")] public string Extension { get; set; }
        [JsonProperty("bloburl")] public string BlobUrl { get; set; }
        [JsonProperty("base64string")] public string Base64String { get; set; }
        [JsonProperty("filesize")] public long FileSize { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("createdon")] public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("filename")] public string Filename { get; set; }
    }
}
