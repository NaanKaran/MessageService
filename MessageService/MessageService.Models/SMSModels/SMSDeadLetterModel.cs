using System;
using Newtonsoft.Json;

namespace MessageService.Models.SMSModels
{
    public class SMSDeadLetterModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("eventTime")]
        public DateTime EventTime { get; set; }
        [JsonProperty("eventType")]
        public string EventType { get; set; }
        [JsonProperty("dataVersion")]
        public string DataVersion { get; set; }
        [JsonProperty("metadataVersion")]
        public string MetadataVersion { get; set; }
        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("deadLetterReason")]
        public string DeadLetterReason { get; set; }
        [JsonProperty("deliveryAttempts")]
        public int DeliveryAttempts { get; set; }
        [JsonProperty("lastDeliveryOutcome")]
        public string LastDeliveryOutcome { get; set; }
        [JsonProperty("lastHttpStatusCode")]
        public int LastHttpStatusCode { get; set; }
        [JsonProperty("publishTime")]
        public DateTime PublishTime { get; set; }
        [JsonProperty("lastDeliveryAttemptTime")]
        public DateTime LastDeliveryAttemptTime { get; set; }
        [JsonProperty("data")]
        public SFSendMessageRequestData Data { get; set; }
    }
}
