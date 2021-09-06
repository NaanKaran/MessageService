using System;
using MessageService.InfraStructure.Helpers;
using Newtonsoft.Json;

namespace MessageService.Models
{
    public class VendorSettingsModel
    {
        [JsonProperty("accountid")]
        public long AccountId { get; set; }
        public string AccountName { get; set; }
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

        // SMS Specific Properties
        [JsonProperty("signaturetext")]
        public string SignatureText { get; set; }
        [JsonProperty("unsubscribetext")]
        public string UnSubscribeText { get; set; }
        [JsonProperty("templateid")]
        public string TemplateId { get; set; }
    }
}
