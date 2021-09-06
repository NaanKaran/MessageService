using System;
using System.Collections.Generic;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.Enum;
using Newtonsoft.Json;

namespace MessageService.Models.CosmosModel
{
    public class TopUpRequestDocumentModel : CosmosBaseModel
    {
        [JsonProperty("partitionkey")] public string PartitionKey => CosmosDocumentType.TopUpRequest.ToString();
        [JsonProperty("accountid")] public long AccountId { get; set; }
        [JsonProperty("raisedbyuserid")] public string RaisedByUserId { get; set; }
        [JsonProperty("raisedbyusername")] public string RaisedByUserName { get; set; }
        [JsonProperty("raisedbyuserrole")] public string RaisedByUserRole { get; set; }
        [JsonProperty("raisedbyuseremailid")] public string RaisedByUserEmailId { get; set; }
        [JsonProperty("raisedon")] public DateTime RaisedOn { get; set; } = DateTime.UtcNow.ToChinaTime();
        [JsonProperty("raisedonstring")] public string RaisedOnString => RaisedOn.ToDateTimeString();
        [JsonProperty("handledbyuserid")] public string HandledByUserId { get; set; }
        [JsonProperty("handledbyusername")] public string HandledByUserName { get; set; }
        [JsonProperty("handledon")] public DateTime? HandledOn { get; set; }
        [JsonProperty("handledonstring")] public string HandledOnString => HandledOn.ToDateTimeString();
        [JsonProperty("topupvalue")] public int TopUpValue { get; set; }
        [JsonProperty("status")] public TopUpRequestStatus Status { get; set; }
        [JsonProperty("type")] public override CosmosDocumentType Type { get; set; } = CosmosDocumentType.TopUpRequest;
        [JsonProperty("requestedtousers")] public List<EmailNotificationUserDocumentModel> RequestedToUsers { get; set; } = new List<EmailNotificationUserDocumentModel>();
        
    }
}
