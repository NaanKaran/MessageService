using System;
using System.Collections.Generic;
using System.Text;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.CosmosModel
{
    public class VerificationSMSDocumentModel : CosmosBaseModel
    {
        [JsonProperty("partitionkey")] public string PartitionKey { get; set; } = CosmosDocumentType.VerificationSMSLog.ToString();
        [JsonProperty("mobilenumber")] public string MobileNumber { get; set; }
        [JsonProperty("openid")] public string OpenId { get; set; }
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("smscontent")] public string SMSContent { get; set; }
        [JsonProperty("senddate")] public DateTime SendDate { get; set; }
        [JsonProperty("senddatestring")] public string SendDateString => SendDate.ToDateTimeString();
        [JsonProperty("followername")] public string FollowerName { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("province")] public string Province { get; set; }
        [JsonProperty("city")] public string City { get; set; }
        [JsonProperty("deliverystatus")] public DeliveryStatus? DeliveryStatus { get; set; }
        [JsonProperty("sendstatus")] public SendStatus? SendStatus { get; set; }
        [JsonProperty("deliverydate")] public DateTime? DeliveryDate { get; set; }
        [JsonProperty("deliverydatestring")] public string DeliveryDateString => DeliveryDate.ToDateTimeString();
        [JsonProperty("droperrorcode")] public string DropErrorCode { get; set; }
        [JsonProperty("type")]  public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.VerificationSMSLog;
    }

    public class VerificationSMSModel
    {
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("openid")] public string OpenId { get; set; }
        [JsonProperty("mobilenumber")] public string MobileNumber { get; set; }
        [JsonProperty("verificationcode")] public string VerificationCode { get; set; } 

    }
}
