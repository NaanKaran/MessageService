using System;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.MMSModels
{
    public class MMSTemplateModel
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("templatename")] public string TemplateName { get; set; }
        [JsonProperty("signature")] public string Signature { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("comments")] public string Comments { get; set; }
        [JsonProperty("isdeleted")] public bool IsDeleted { get; set; }
        [JsonProperty("content")]  public string Content { get; set; }
        [JsonProperty("variables")] public string Variables { get; set; }
        [JsonProperty("status")] public TemplateStatus Status { get; set; }
        [JsonProperty("createdby")] public string CreatedBy { get; set; }
        [JsonProperty("createdon")] public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("createdonstring")] public string CreatedOnString => CreatedOn.ToDateTimeString(); 
        [JsonProperty("updatedby")] public string UpdatedBy { get; set; }
        [JsonProperty("updatedon")] public DateTime? UpdatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
    }

    public class MMSContent
    {
        [JsonProperty("pageno")] public int PageNo { get; set; }
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("bloburl")] public string BlobUrl { get; set; }
        [JsonProperty("base64string")] public string Base64String { get; set; }
        [JsonProperty("filesize")] public string FileSize { get; set; }
        [JsonProperty("filetype")] public string FileType { get; set; }
        [JsonProperty("extension")] public string Extension { get; set; }
        [JsonProperty("type")] public MediaType Type { get; set; } = MediaType.image;
    }

    public class TemplateJourneyDropDownModel
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("templatename")] public string TemplateName { get; set; }
        [JsonProperty("variables")] public string Variables { get; set; }
    }
}
