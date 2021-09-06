using System;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.CosmosModel
{
    public class SMSErrorCodeDetailsDocumentModel : CosmosBaseModel
    {
        [JsonProperty("partitionkey")] public string PartitionKey => CosmosDocumentType.ErrorCodeDetails.ToString();
        [JsonProperty("englishdescription")] public string EnglishDescription { get; set; }
        [JsonProperty("chinesedescription")]  public string ChineseDescription { get; set; }
        [JsonProperty("errorcode")]  public string ErrorCode { get; set; }
        [JsonProperty("vendorid")]  public VendorType VendorId { get; set; }
        [JsonProperty("type")] public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.ErrorCodeDetails;
        [JsonProperty("createdon")] public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
