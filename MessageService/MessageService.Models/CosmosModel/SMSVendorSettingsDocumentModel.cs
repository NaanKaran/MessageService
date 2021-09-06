using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;
using System;

namespace MessageService.Models.CosmosModel
{
    public class SMSVendorSettingsDocumentModel : CosmosBaseModel
    {
        [JsonProperty("partitionkey")] public string PartitionKey => CosmosDocumentType.VendorSetting.ToString();
        [JsonProperty("accountid")]
        public long AccountId { get; set; }
        [JsonProperty("vendorid")]
        public short VendorId { get; set; }
        [JsonProperty("appid")]
        public string AppId { get; set; }
        [JsonProperty("appkey")]
        public string AppKey { get; set; }
        [JsonIgnore]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonIgnore]
        public DateTime? UpdatedOn { get; set; } = DateTime.UtcNow.ToChinaTime();

        [JsonProperty("signaturetext")]
        public string SignatureText { get; set; }
        [JsonProperty("verificationtemplateid")]
        public string VerificationTemplateId { get; set; }
        [JsonProperty("unsubscribetext")]
        public string UnSubscribeText { get; set; }
        [JsonProperty("templateid")]
        public string TemplateId { get; set; }
        [JsonProperty("category")]
        public string Category { get; set; }
        [JsonProperty("type")] public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.VendorSetting;        
    }
}
