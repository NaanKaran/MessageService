using System;
using System.Collections.Generic;
using System.Linq;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.CosmosModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MessageService.Models
{

    public class SFSendMessageRequestData
    {
        [JsonProperty("correlationId")]
        public Guid CorrelationId { get; set; }
        [JsonProperty("activityObjectID")]
        public string ActivityObjectId { get; set; }
        [JsonProperty("journeyId")] //as per our db
        public string InteractionId { get; set; }
        [JsonProperty("activityId")]
        public string ActivityId { get; set; }
        [JsonProperty("definitionInstanceId")]
        public string DefinitionInstanceId { get; set; }
        [JsonProperty("activityInstanceId")]
        public string ActivityInstanceId { get; set; }
        [JsonProperty("keyValue")]
        public string KeyValue { get; set; }
        [JsonProperty("mode")]
        public int Mode { get; set; }
        [JsonProperty("inArguments")]
        public List<InArgument> InArguments { get; set; } = new List<InArgument>();
        [JsonProperty("outArguments")]
        public List<object> OutArguments { get; set; }

    }

    public class InArgument
    {
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("contactKey")] public string ContactKey { get; set; }
        [JsonProperty("sendAccount")] public string SendAccount { get; set; }
        public long AccountId => Convert.ToInt64(SendAccount ?? "0");
        [JsonProperty("phoneNumber")] public string MobileNumber { get; set; }
        [JsonProperty("orgId")] public string OrgId { get; set; }
        [JsonProperty("journeyVersion")] public string JourneyVersion { get; set; }
        [JsonProperty("activityId")] public string ActivityId { get; set; }
        [JsonProperty("actionName")] public string ActionName { get; set; }
        [JsonProperty("mmstemplateId")] public string TemplateId { get; set; }
        [JsonProperty("variables")] public JArray Variables { get; set; }
        public Dictionary<string, string> VariablesDic { get; set; }
        [JsonProperty("dataextensionvariables")] public JArray DataExtensionVariables { get; set; }
        public Dictionary<string, string> DataExtensionVariablesDic => DataExtensionVariables.ToDictionary();
        [JsonProperty("vendorSettings")] public VendorSettingsModel VendorSettings { get; set; }
        [JsonProperty("smsVendorSettings")] public SMSVendorSettingsDocumentModel SMSVendorSettings { get; set; }

        // SMS Specific Properties
        [JsonProperty("sendBot")] public string SMSContent { get; set; }
        [JsonProperty("journeyKey")] public string JourneyKey { get; set; }

        [JsonProperty("customData")] public JObject SMSPersonalizationData { get; set; }
        [JsonProperty("enrtySource")] public string EntrySource { get; set; }
        [JsonProperty("enrtySourceContactKey")] public string EntrySourceContactKey { get; set; }
        [JsonProperty("smsPersonalizationVariables")]
        public Dictionary<string, string> SMSPersonalizationVariables => SMSPersonalizationData.ToDictionary();
    }
}
